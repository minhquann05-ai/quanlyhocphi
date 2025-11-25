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
            // 1. Tìm thông tin sinh viên để biết họ thuộc Ngành nào
            var sinhVien = await _context.SinhViens
                .Include(sv => sv.LopHoc) // Phải Include Lớp để lấy Mã Ngành
                .AsNoTracking()
                .FirstOrDefaultAsync(sv => sv.MaSv == maSv);

            if (sinhVien == null)
            {
                throw new Exception("Không tìm thấy thông tin sinh viên.");
            }

            // Kiểm tra nếu sinh viên chưa được phân lớp hoặc lớp chưa có ngành
            if (sinhVien.LopHoc == null || string.IsNullOrEmpty(sinhVien.LopHoc.MaNganh))
            {
                // Trả về danh sách rỗng hoặc chỉ môn NHC tùy chính sách
                // Ở đây ta báo lỗi để admin biết mà phân lớp
                throw new Exception("Sinh viên chưa được phân vào lớp chuyên ngành nào, không thể xác định môn học được phép đăng ký.");
            }

            string maNganhCuaSv = sinhVien.LopHoc.MaNganh; // Ví dụ: "CNTT"

            // 2. Tìm Học kỳ đang mở ("Chuẩn bị mở")
            var activeHocKy = await _context.HocKys
                .AsNoTracking()
                .FirstOrDefaultAsync(hk => hk.TrangThai == "Chuẩn bị mở");

            if (activeHocKy == null)
            {
                return new List<LopHocPhanDto>(); // Không có kỳ nào mở thì danh sách trống
            }

            // 3. Truy vấn và Lọc dữ liệu
            var lopHocPhans = await _context.LopHocPhans
                .Include(lhp => lhp.MonHoc)
                .Where(lhp =>
                    // Điều kiện 1: Phải thuộc học kỳ đang mở
                    lhp.MaHk == activeHocKy.MaHk
                    &&
                    // Điều kiện 2: Môn học phải thuộc Ngành của SV HOẶC là Ngành học chung (NHC)
                    (lhp.MonHoc.MaNganh == maNganhCuaSv || lhp.MonHoc.MaNganh == "NHC")
                )
                .OrderBy(lhp => lhp.MonHoc.TenMh) // Sắp xếp theo tên môn cho dễ nhìn
                .AsNoTracking()
                .ToListAsync();

            return _mapper.Map<IEnumerable<LopHocPhanDto>>(lopHocPhans);
        }
        public async Task CancelRegistrationAsync(string maSv, string maLhp, string role = null)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Tìm bản ghi đăng ký
                var dangKy = await _context.DangKyHocPhans
                    .Include(dk => dk.LopHocPhan)
                    .FirstOrDefaultAsync(dk => dk.MaSv == maSv && dk.MaLhp == maLhp);

                if (dangKy == null)
                    throw new KeyNotFoundException("Bạn chưa đăng ký lớp học phần này.");

                // 2. Tìm Hóa đơn
                var hoaDon = await _context.HoaDons
                    .Include(hd => hd.ChiTietHoaDons)
                    .FirstOrDefaultAsync(hd => hd.MaSv == maSv && hd.MaHk == dangKy.MaHk);

                // 3. Kiểm tra trạng thái thanh toán (LOGIC MỚI)
                if (hoaDon != null)
                {
                    // Chỉ chặn nếu là Sinh Viên và Hóa đơn đã thanh toán
                    // Nếu là Phòng Tài Chính (role == "PhongTaiChinh") thì cho qua luôn
                    if (hoaDon.TrangThai == "Đã thanh toán" && role != "PhongTaiChinh")
                    {
                        throw new Exception("Không thể hủy lớp này vì bạn ĐÃ THANH TOÁN học phí. Vui lòng liên hệ Phòng Tài Chính để được hỗ trợ hoàn tiền.");
                    }

                    // ... (Phần xóa tiền trong hóa đơn giữ nguyên) ...
                    // Copy đoạn code tìm và xóa chi tiết hóa đơn cũ vào đây
                    var chiTietToRemove = hoaDon.ChiTietHoaDons
                        .FirstOrDefault(ct => ct.NoiDung.Contains(dangKy.LopHocPhan.TenLhp));

                    if (chiTietToRemove != null)
                    {
                        hoaDon.TongTien -= chiTietToRemove.SoTien;
                        if (hoaDon.TongTien < 0) hoaDon.TongTien = 0;
                        _context.ChiTietHoaDons.Remove(chiTietToRemove);
                    }

                    // Nếu hủy xong mà hóa đơn rỗng -> Xóa luôn hóa đơn (hoặc cập nhật trạng thái nếu Admin muốn)
                    if (hoaDon.ChiTietHoaDons.Count <= 1 && chiTietToRemove != null)
                    {
                        _context.HoaDons.Remove(hoaDon);
                    }
                    // Nếu Admin hủy hóa đơn đã thanh toán -> Có thể cần cập nhật lại trạng thái hóa đơn về "Chưa thanh toán" hoặc xử lý hoàn tiền (tùy nghiệp vụ sâu hơn)
                    // Ở mức độ này, ta cứ trừ tiền bình thường.
                }

                // ... (Phần giảm sĩ số và xóa đăng ký giữ nguyên) ...
                var lopHocPhan = await _context.LopHocPhans.FindAsync(maLhp);
                if (lopHocPhan != null)
                {
                    lopHocPhan.SiSoThucTe--;
                    if (lopHocPhan.SiSoThucTe < 0) lopHocPhan.SiSoThucTe = 0;
                    _context.LopHocPhans.Update(lopHocPhan);
                }

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
        public async Task<IEnumerable<KetQuaDangKyDto>> GetRegisteredClassesAsync(string maSv)
        {
            // 1. Tìm học kỳ đang mở (Chỉ xem kết quả của kỳ hiện tại)
            var activeHocKy = await _context.HocKys
                .AsNoTracking()
                .FirstOrDefaultAsync(hk => hk.TrangThai == "Chuẩn bị mở");

            if (activeHocKy == null) return new List<KetQuaDangKyDto>();

            // 2. Lấy danh sách lớp SV đã đăng ký trong kỳ này
            var listDangKy = await _context.DangKyHocPhans
                .Include(dk => dk.LopHocPhan)
                    .ThenInclude(l => l.MonHoc)
                .Where(dk => dk.MaSv == maSv && dk.MaHk == activeHocKy.MaHk)
                .OrderBy(dk => dk.NgayDk)
                .AsNoTracking()
                .ToListAsync();

            var result = _mapper.Map<List<KetQuaDangKyDto>>(listDangKy);

            // 3. Tính học phí (Optional: Để hiển thị cho đẹp)
            // Lấy biểu phí của kỳ này để tính tiền từng môn
            var listBieuPhi = await _context.BieuPhis
                .Where(bp => bp.MaHk == activeHocKy.MaHk)
                .ToListAsync();

            foreach (var item in result)
            {
                // Tìm lại môn học để biết thuộc ngành nào -> Lấy giá
                // (Cách nhanh nhất là query lại hoặc map thêm MaNganh vào DTO, ở đây làm nhanh)
                var monHoc = listDangKy.First(x => x.MaLhp == item.MaLhp).LopHocPhan.MonHoc;
                var bieuPhi = listBieuPhi.FirstOrDefault(bp => bp.MaNganh == monHoc.MaNganh);

                if (bieuPhi != null)
                {
                    item.HocPhi = item.SoTinChi * bieuPhi.DonGiaTinChi;
                }
            }

            return result;
        }
    }
}