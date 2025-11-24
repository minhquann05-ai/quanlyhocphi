using AutoMapper;
using Microsoft.EntityFrameworkCore;
using QLHocPhi.BLL.Interfaces;
using QLHocPhi.Common.Dtos;
using QLHocPhi.DAL;
using QLHocPhi.DAL.Entities;

namespace QLHocPhi.BLL.Services
{
    public class DangKyHocPhanService : IDangKyHocPhanService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        public DangKyHocPhanService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<HoaDonDto> CreateDangKyAsync(DangKyHocPhanCreateDto createDto)
        {
            if (string.IsNullOrEmpty(createDto.MaSv))
            {
                throw new Exception("Mã sinh viên không hợp lệ.");
            }
            var classesRequested = await _context.LopHocPhans
            .Where(l => createDto.ListMaLhp.Contains(l.MaLhp))
            .Include(l => l.MonHoc)
            .ToListAsync();

            // Kiểm tra xem có môn nào xuất hiện > 1 lần không
            var duplicateSubject = classesRequested
                .GroupBy(c => c.MaMh) // Gom nhóm theo Mã Môn Học
                .FirstOrDefault(g => g.Count() > 1); // Tìm nhóm nào có số lượng > 1

            if (duplicateSubject != null)
            {
                var tenMon = duplicateSubject.First().MonHoc.TenMh;
                throw new Exception($"Trong danh sách đăng ký có 2 lớp cùng thuộc môn '{tenMon}'. Bạn chỉ được chọn 1 lớp cho mỗi môn.");
            }
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var activeHocKy = await _context.HocKys
                    .AsNoTracking()
                    .FirstOrDefaultAsync(hk => hk.TrangThai == "Chuẩn bị mở");

                if (activeHocKy == null)
                    throw new Exception("Không có học kỳ nào đang mở để đăng ký.");

                var chiTietHoaDonList = new List<ChiTietHoaDonCreateDto>();
                var lastDk = await _context.DangKyHocPhans.OrderByDescending(dk => dk.MaDk).FirstOrDefaultAsync();
                int nextDkId = 1;
                if (lastDk != null && lastDk.MaDk.StartsWith("DK"))
                {
                    if (int.TryParse(lastDk.MaDk.Substring(2), out int lastId))
                        nextDkId = lastId + 1;
                }

                foreach (var maLhp in createDto.ListMaLhp)
                {
                    // A. TÌM LỚP HỌC PHẦN
                    // Lưu ý: Phải dùng Lock hoặc Transaction cấp cao để tránh 2 người cùng đăng ký slot cuối cùng.
                    // Nhưng ở mức đồ án, chỉ cần check code bình thường là được.
                    var lopHocPhan = await _context.LopHocPhans
                        .Include(lhp => lhp.MonHoc) // Kèm môn học để lấy số tín chỉ
                        .FirstOrDefaultAsync(l => l.MaLhp == maLhp);

                    if (lopHocPhan == null)
                        throw new KeyNotFoundException($"Lớp học phần {maLhp} không tồn tại.");

                    // B. KIỂM TRA SLOT (QUAN TRỌNG)
                    if (lopHocPhan.SiSoThucTe >= lopHocPhan.SiSoToiDa)
                    {
                        throw new Exception($"Lớp {lopHocPhan.TenLhp} đã hết chỗ (Full slot).");
                    }

                    // C. TĂNG SĨ SỐ LÊN 1
                    lopHocPhan.SiSoThucTe += 1;
                    _context.LopHocPhans.Update(lopHocPhan);

                    // D. KIỂM TRA TRÙNG (Sinh viên đã học lớp này chưa)
                    var daDangKyMonNay = await _context.DangKyHocPhans
        .Include(dk => dk.LopHocPhan) // Join sang bảng Lớp để lấy Mã Môn
        .AnyAsync(dk => dk.MaSv == createDto.MaSv
                     && dk.MaHk == activeHocKy.MaHk
                     && dk.LopHocPhan.MaMh == lopHocPhan.MaMh); // So sánh theo Mã Môn Học

                    if (daDangKyMonNay)
                    {
                        throw new Exception($"Bạn đã đăng ký môn '{lopHocPhan.MonHoc.TenMh}' rồi (có thể ở lớp khác). Không được đăng ký trùng môn.");
                    }

                    // E. TÌM BIỂU PHÍ (Theo ngành của môn học)
                    var bieuPhi = await _context.BieuPhis
                        .AsNoTracking()
                        .FirstOrDefaultAsync(bp => bp.MaNganh == lopHocPhan.MonHoc.MaNganh && bp.MaHk == activeHocKy.MaHk);

                    if (bieuPhi == null) throw new Exception($"Chưa có biểu phí cho lớp {lopHocPhan.TenLhp}.");

                    // F. TẠO RECORD ĐĂNG KÝ
                    var dangKy = new DangKyHocPhan
                    {
                        MaSv = createDto.MaSv,
                        MaLhp = maLhp, // Lưu mã lớp
                        MaHk = activeHocKy.MaHk,
                        NgayDk = DateTime.UtcNow,
                        TrangThai = "Đã đăng ký",
                        MaDk = $"DK{nextDkId:D4}"
                    };
                    _context.DangKyHocPhans.Add(dangKy);
                    nextDkId++;

                    // G. TÍNH TIỀN
                    var soTienMonHoc = (lopHocPhan.MonHoc.SoTinChi ?? 0) * bieuPhi.DonGiaTinChi;
                    chiTietHoaDonList.Add(new ChiTietHoaDonCreateDto
                    {
                        NoiDung = $"Học phí lớp: {lopHocPhan.TenLhp} ({lopHocPhan.MonHoc.SoTinChi} tín chỉ)",
                        SoTien = soTienMonHoc
                    });
                }

                if (chiTietHoaDonList.Count == 0)
                    throw new Exception("Không có môn học nào hợp lệ để tạo hóa đơn.");

                var hoaDon = new HoaDon
                {
                    MaSv = createDto.MaSv,
                    MaHk = activeHocKy.MaHk,
                    TrangThai = "Chưa thanh toán",
                    NgayTao = DateTime.UtcNow,
                    TongTien = chiTietHoaDonList.Sum(ct => ct.SoTien) 
                };

                var lastHoaDon = await _context.HoaDons.OrderByDescending(hd => hd.MaHd).FirstOrDefaultAsync();
                int nextHdId = 1;
                if (lastHoaDon != null && lastHoaDon.MaHd.StartsWith("HD"))
                {
                    if (int.TryParse(lastHoaDon.MaHd.Substring(2), out int lastId))
                        nextHdId = lastId + 1;
                }
                hoaDon.MaHd = $"HD{nextHdId:D4}";
                _context.HoaDons.Add(hoaDon);

                var lastCt = await _context.ChiTietHoaDons.OrderByDescending(ct => ct.MaCt).FirstOrDefaultAsync();
                int nextCtId = 1;
                if (lastCt != null && lastCt.MaCt.StartsWith("CT"))
                {
                    if (int.TryParse(lastCt.MaCt.Substring(2), out int lastId))
                        nextCtId = lastId + 1;
                }

                var chiTietDtoList = new List<ChiTietHoaDonDto>();

                foreach (var chiTietDto in chiTietHoaDonList)
                {
                    var chiTiet = _mapper.Map<ChiTietHoaDon>(chiTietDto);
                    chiTiet.MaHd = hoaDon.MaHd;
                    chiTiet.MaCt = $"CT{nextCtId:D4}";
                    _context.ChiTietHoaDons.Add(chiTiet);

                    chiTietDtoList.Add(_mapper.Map<ChiTietHoaDonDto>(chiTiet));
                    nextCtId++;
                }

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                var sinhVien = await _context.SinhViens.FindAsync(createDto.MaSv);
                return new HoaDonDto
                {
                    MaHd = hoaDon.MaHd,
                    MaSv = hoaDon.MaSv,
                    TenSv = sinhVien?.HoTen,
                    MaHk = hoaDon.MaHk,
                    TenHk = activeHocKy.TenHk,
                    NgayTao = hoaDon.NgayTao,
                    TongTien = hoaDon.TongTien,
                    TrangThai = hoaDon.TrangThai,
                    ChiTiet = chiTietDtoList
                };
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task<IEnumerable<LopHocPhanDto>> GetAvailableClassesForStudentAsync(string maSv)
        {
            // 1. Tìm thông tin sinh viên để biết Ngành
            var sinhVien = await _context.SinhViens
                .Include(sv => sv.LopHoc) // Để lấy Mã Ngành
                .AsNoTracking()
                .FirstOrDefaultAsync(sv => sv.MaSv == maSv);

            if (sinhVien == null || sinhVien.LopHoc == null)
                throw new Exception("Không tìm thấy thông tin ngành học của sinh viên.");

            string maNganhCuaSv = sinhVien.LopHoc.MaNganh;

            // 2. Tìm Học kỳ đang mở ("Chuẩn bị mở")
            // (Vì khi đăng ký là đăng ký cho kỳ sắp tới)
            var activeHocKy = await _context.HocKys
                .AsNoTracking()
                .FirstOrDefaultAsync(hk => hk.TrangThai == "Chuẩn bị mở");

            if (activeHocKy == null) return new List<LopHocPhanDto>(); // Không có kỳ nào mở

            // 3. Truy vấn các lớp học phần thỏa mãn 2 điều kiện:
            // - Thuộc học kỳ đang mở
            // - Môn học thuộc ngành của sinh viên (hoặc môn chung "NHC")
            var lopHocPhans = await _context.LopHocPhans
                .Include(lhp => lhp.MonHoc)
                .Where(lhp => lhp.MaHk == activeHocKy.MaHk // Đúng kỳ
                           && (lhp.MonHoc.MaNganh == maNganhCuaSv || lhp.MonHoc.MaNganh == "NHC")) // Đúng ngành hoặc môn chung
                .OrderBy(lhp => lhp.MonHoc.TenMh)
                .AsNoTracking()
                .ToListAsync();

            return _mapper.Map<IEnumerable<LopHocPhanDto>>(lopHocPhans);
        }
        public async Task CancelRegistrationAsync(string maSv, string maLhp)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Tìm bản ghi đăng ký
                var dangKy = await _context.DangKyHocPhans
                    .Include(dk => dk.LopHocPhan) // Để lấy tên lớp (so khớp hóa đơn)
                    .FirstOrDefaultAsync(dk => dk.MaSv == maSv && dk.MaLhp == maLhp);

                if (dangKy == null)
                    throw new KeyNotFoundException("Bạn chưa đăng ký lớp học phần này.");

                // 2. Tìm Hóa đơn của học kỳ đó
                // (Giả định mỗi kỳ sinh viên chỉ có 1 hóa đơn học phí chính)
                var hoaDon = await _context.HoaDons
                    .Include(hd => hd.ChiTietHoaDons)
                    .FirstOrDefaultAsync(hd => hd.MaSv == maSv && hd.MaHk == dangKy.MaHk);

                // 3. Kiểm tra trạng thái thanh toán
                if (hoaDon != null)
                {
                    if (hoaDon.TrangThai == "Đã thanh toán")
                    {
                        throw new Exception("Không thể hủy lớp này vì bạn ĐÃ THANH TOÁN học phí. Vui lòng liên hệ Phòng Tài Chính để được hỗ trợ hoàn tiền.");
                    }

                    // 4. Tìm và Xóa dòng tiền trong chi tiết hóa đơn
                    // (Dựa vào chuỗi nội dung chúng ta đã tạo: "Học phí lớp: {TenLhp}...")
                    var chiTietToRemove = hoaDon.ChiTietHoaDons
                        .FirstOrDefault(ct => ct.NoiDung.Contains(dangKy.LopHocPhan.TenLhp));

                    if (chiTietToRemove != null)
                    {
                        // Trừ tiền
                        hoaDon.TongTien -= chiTietToRemove.SoTien;
                        if (hoaDon.TongTien < 0) hoaDon.TongTien = 0;

                        // Xóa chi tiết
                        _context.ChiTietHoaDons.Remove(chiTietToRemove);
                    }

                    // Nếu hóa đơn không còn đồng nào (do hủy hết môn) -> Xóa luôn hóa đơn cho sạch
                    // (Logic: Nếu list chi tiết chỉ còn đúng 1 cái mình vừa xóa)
                    if (hoaDon.ChiTietHoaDons.Count <= 1 && chiTietToRemove != null)
                    {
                        _context.HoaDons.Remove(hoaDon);
                    }
                }

                // 5. Cập nhật Sĩ số lớp (-1)
                var lopHocPhan = await _context.LopHocPhans.FindAsync(maLhp);
                if (lopHocPhan != null)
                {
                    lopHocPhan.SiSoThucTe--;
                    if (lopHocPhan.SiSoThucTe < 0) lopHocPhan.SiSoThucTe = 0; // An toàn
                    _context.LopHocPhans.Update(lopHocPhan);
                }

                // 6. Xóa bản ghi Đăng ký
                _context.DangKyHocPhans.Remove(dangKy);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}