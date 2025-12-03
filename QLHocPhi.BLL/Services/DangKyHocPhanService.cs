using AutoMapper;
using Microsoft.EntityFrameworkCore;
using QLHocPhi.BLL.Interfaces;
using QLHocPhi.Common.Dtos;
using QLHocPhi.DAL;
using QLHocPhi.DAL.Entities;

namespace QuanLyHocPhi.BLL.Services
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

        public async Task<IEnumerable<LopHocPhanDto>> GetAvailableClassesForStudentAsync(string maSv)
        {
            var sinhVien = await _context.SinhViens.Include(sv => sv.LopHoc).AsNoTracking().FirstOrDefaultAsync(sv => sv.MaSv == maSv);
            if (sinhVien == null || sinhVien.LopHoc == null) throw new Exception("Không tìm thấy thông tin sinh viên.");

            string maNganhCuaSv = sinhVien.LopHoc.MaNganh;
            var activeHocKy = await _context.HocKys.AsNoTracking().FirstOrDefaultAsync(hk => hk.TrangThai == "Chuẩn bị mở");

            if (activeHocKy == null) return new List<LopHocPhanDto>();

            var lopHocPhans = await _context.LopHocPhans
                .Include(lhp => lhp.MonHoc)
                .Where(lhp => lhp.MaHk == activeHocKy.MaHk
                           && (lhp.MonHoc.MaNganh == maNganhCuaSv || lhp.MonHoc.MaNganh == "NHC"))
                .OrderBy(lhp => lhp.MonHoc.TenMh)
                .AsNoTracking()
                .ToListAsync();

            return _mapper.Map<IEnumerable<LopHocPhanDto>>(lopHocPhans);
        }

        public async Task<IEnumerable<KetQuaDangKyDto>> GetRegisteredClassesAsync(string maSv)
        {
            var activeHocKy = await _context.HocKys.AsNoTracking().FirstOrDefaultAsync(hk => hk.TrangThai == "Chuẩn bị mở");
            if (activeHocKy == null) return new List<KetQuaDangKyDto>();

            var listDangKy = await _context.DangKyHocPhans
                .Include(dk => dk.LopHocPhan).ThenInclude(l => l.MonHoc)
                .Where(dk => dk.MaSv == maSv && dk.MaHk == activeHocKy.MaHk)
                .OrderBy(dk => dk.NgayDk)
                .AsNoTracking().ToListAsync();

            var result = _mapper.Map<List<KetQuaDangKyDto>>(listDangKy);
            var listBieuPhi = await _context.BieuPhis.Where(bp => bp.MaHk == activeHocKy.MaHk).ToListAsync();

            foreach (var item in result)
            {
                var monHoc = listDangKy.First(x => x.MaLhp == item.MaLhp).LopHocPhan.MonHoc;
                var bieuPhi = listBieuPhi.FirstOrDefault(bp => bp.MaNganh == monHoc.MaNganh);
                if (bieuPhi != null) item.HocPhi = item.SoTinChi * bieuPhi.DonGiaTinChi;
            }
            return result;
        }

        public async Task<HoaDonDto> CreateDangKyAsync(DangKyHocPhanCreateDto createDto)
        {
            if (string.IsNullOrEmpty(createDto.MaSv)) throw new Exception("Mã sinh viên không hợp lệ.");

            var classesRequested = await _context.LopHocPhans
                .Where(l => createDto.ListMaLhp.Contains(l.MaLhp))
                .Include(l => l.MonHoc).ToListAsync();

            if (classesRequested.GroupBy(c => c.MaMh).Any(g => g.Count() > 1))
                throw new Exception($"Không được chọn 2 lớp cùng 1 môn trong một lần đăng ký.");

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var activeHocKy = await _context.HocKys.AsNoTracking().FirstOrDefaultAsync(hk => hk.TrangThai == "Chuẩn bị mở");
                if (activeHocKy == null) throw new Exception("Không có học kỳ nào đang mở.");

                var chiTietMoiList = new List<ChiTietHoaDonCreateDto>();
                var lastDk = await _context.DangKyHocPhans.OrderByDescending(dk => dk.MaDk).FirstOrDefaultAsync();
                int nextDkId = 1;
                if (lastDk != null && lastDk.MaDk.StartsWith("DK") && int.TryParse(lastDk.MaDk.Substring(2), out int lastId)) nextDkId = lastId + 1;

                foreach (var maLhp in createDto.ListMaLhp)
                {
                    var lopHocPhan = await _context.LopHocPhans.Include(l => l.MonHoc).FirstOrDefaultAsync(l => l.MaLhp == maLhp);
                    if (lopHocPhan == null) throw new KeyNotFoundException($"Lớp {maLhp} không tồn tại.");

                    if (lopHocPhan.SiSoThucTe >= lopHocPhan.SiSoToiDa) throw new Exception($"Lớp {lopHocPhan.TenLhp} đã hết chỗ.");

                    var daDangKy = await _context.DangKyHocPhans
                        .Include(dk => dk.LopHocPhan)
                        .AnyAsync(dk => dk.MaSv == createDto.MaSv && dk.MaHk == activeHocKy.MaHk && dk.LopHocPhan.MaMh == lopHocPhan.MaMh);
                    if (daDangKy) throw new Exception($"Bạn đã đăng ký môn '{lopHocPhan.MonHoc.TenMh}' rồi.");

                    var dangKy = new DangKyHocPhan
                    {
                        MaDk = $"DK{nextDkId:D4}",
                        MaSv = createDto.MaSv,
                        MaLhp = maLhp,
                        MaHk = activeHocKy.MaHk,
                        NgayDk = DateTime.UtcNow,
                        TrangThai = "Đã đăng ký"
                    };
                    _context.DangKyHocPhans.Add(dangKy);
                    nextDkId++;

                    lopHocPhan.SiSoThucTe += 1;

                    var bieuPhi = await _context.BieuPhis.AsNoTracking().FirstOrDefaultAsync(bp => bp.MaNganh == lopHocPhan.MonHoc.MaNganh && bp.MaHk == activeHocKy.MaHk);
                    if (bieuPhi == null) throw new Exception($"Chưa có biểu phí cho lớp {lopHocPhan.TenLhp}.");

                    chiTietMoiList.Add(new ChiTietHoaDonCreateDto
                    {
                        NoiDung = $"Học phí lớp: {lopHocPhan.TenLhp} ({lopHocPhan.MonHoc.SoTinChi} tín chỉ)",
                        SoTien = (lopHocPhan.MonHoc.SoTinChi ?? 0) * bieuPhi.DonGiaTinChi
                    });
                }

                var hoaDon = await _context.HoaDons
                    .Include(hd => hd.ChiTietHoaDons)
                    .FirstOrDefaultAsync(hd => hd.MaSv == createDto.MaSv && hd.MaHk == activeHocKy.MaHk && hd.TrangThai == "Chưa thanh toán");

                if (hoaDon == null)
                {
                    hoaDon = new HoaDon
                    {
                        MaSv = createDto.MaSv,
                        MaHk = activeHocKy.MaHk,
                        TrangThai = "Chưa thanh toán",
                        NgayTao = DateTime.UtcNow,
                        TongTien = 0     
                    };

                    var lastHd = await _context.HoaDons.OrderByDescending(h => h.MaHd).FirstOrDefaultAsync();
                    int nextHdId = 1;
                    if (lastHd != null && int.TryParse(lastHd.MaHd.Substring(2), out int idHd)) nextHdId = idHd + 1;
                    hoaDon.MaHd = $"HD{nextHdId:D4}";

                    _context.HoaDons.Add(hoaDon);
                }

                var lastCt = await _context.ChiTietHoaDons.OrderByDescending(c => c.MaCt).FirstOrDefaultAsync();
                int nextCtId = 1;
                if (lastCt != null && int.TryParse(lastCt.MaCt.Substring(2), out int idCt)) nextCtId = idCt + 1;

                foreach (var ctDto in chiTietMoiList)
                {
                    var chiTiet = _mapper.Map<ChiTietHoaDon>(ctDto);
                    chiTiet.MaHd = hoaDon.MaHd;
                    chiTiet.MaCt = $"CT{nextCtId:D4}";
                    _context.ChiTietHoaDons.Add(chiTiet);

                    hoaDon.TongTien = (hoaDon.TongTien ?? 0) + chiTiet.SoTien;
                    nextCtId++;
                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new HoaDonDto { MaHd = hoaDon.MaHd, MaSv = hoaDon.MaSv };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task CancelRegistrationAsync(string maSv, string maLhp, string role = null)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var dangKy = await _context.DangKyHocPhans
                    .Include(dk => dk.LopHocPhan).ThenInclude(l => l.MonHoc)
                    .FirstOrDefaultAsync(dk => dk.MaSv == maSv && dk.MaLhp == maLhp);

                if (dangKy == null) throw new KeyNotFoundException("Bạn chưa đăng ký lớp này.");

                string maHk = dangKy.MaHk;
                string tenLopCanXoa = dangKy.LopHocPhan.TenLhp;        

                _context.DangKyHocPhans.Remove(dangKy);
                await _context.SaveChangesAsync();         

                var lopHocPhan = await _context.LopHocPhans.FindAsync(maLhp);
                if (lopHocPhan != null)
                {
                    lopHocPhan.SiSoThucTe--;
                    if (lopHocPhan.SiSoThucTe < 0) lopHocPhan.SiSoThucTe = 0;
                    _context.LopHocPhans.Update(lopHocPhan);
                }

                var hoaDon = await _context.HoaDons
                    .Include(hd => hd.ChiTietHoaDons)
                    .FirstOrDefaultAsync(hd => hd.MaSv == maSv && hd.MaHk == maHk && hd.TrangThai != "Đã hủy");

                if (hoaDon != null)
                {
                    if (hoaDon.TrangThai == "Đã thanh toán" && role != "PhongTaiChinh")
                        throw new Exception("Không thể hủy lớp đã thanh toán.");

                    var chiTietToRemove = hoaDon.ChiTietHoaDons
                        .FirstOrDefault(ct => ct.NoiDung != null && ct.NoiDung.ToLower().Contains(tenLopCanXoa.ToLower().Trim()));

                    if (chiTietToRemove != null)
                    {
                        _context.ChiTietHoaDons.Remove(chiTietToRemove);
                        hoaDon.ChiTietHoaDons.Remove(chiTietToRemove);      
                    }

                    var cacMonConLai = await _context.DangKyHocPhans
                        .Include(dk => dk.LopHocPhan).ThenInclude(l => l.MonHoc)
                        .Where(dk => dk.MaSv == maSv && dk.MaHk == maHk)
                        .ToListAsync();

                    if (cacMonConLai.Count == 0)
                    {
                        hoaDon.TongTien = 0;
                        hoaDon.TrangThai = "Đã hủy";     

                        var allDetails = await _context.ChiTietHoaDons.Where(ct => ct.MaHd == hoaDon.MaHd).ToListAsync();
                        _context.ChiTietHoaDons.RemoveRange(allDetails);
                    }
                    else
                    {
                        decimal tongTienMoi = 0;
                        var bieuPhis = await _context.BieuPhis.Where(bp => bp.MaHk == maHk).ToListAsync();

                        foreach (var mon in cacMonConLai)
                        {
                            var bp = bieuPhis.FirstOrDefault(b => b.MaNganh == mon.LopHocPhan.MonHoc.MaNganh);
                            if (bp != null)
                            {
                                tongTienMoi += (mon.LopHocPhan.MonHoc.SoTinChi ?? 0) * bp.DonGiaTinChi;
                            }
                        }
                        hoaDon.TongTien = tongTienMoi;
                    }

                    if (hoaDon.NgayTao.HasValue)
                        hoaDon.NgayTao = DateTime.SpecifyKind(hoaDon.NgayTao.Value, DateTimeKind.Utc);

                    _context.HoaDons.Update(hoaDon);
                }

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