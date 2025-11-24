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

                foreach (var maMh in createDto.ListMaMh)
                {
                    var monHoc = await _context.MonHocs
                        .AsNoTracking()
                        .FirstOrDefaultAsync(mh => mh.MaMh == maMh);

                    if (monHoc == null)
                        throw new KeyNotFoundException($"Môn học {maMh} không tồn tại.");

                    if (monHoc.SoTinChi == null || monHoc.SoTinChi <= 0)
                        throw new Exception($"Môn học {monHoc.TenMh} không có số tín chỉ hợp lệ.");

                    var bieuPhi = await _context.BieuPhis
                        .AsNoTracking()
                        .FirstOrDefaultAsync(bp => bp.MaNganh == monHoc.MaNganh && bp.MaHk == activeHocKy.MaHk);

                    if (bieuPhi == null)
                        throw new Exception($"Chưa có biểu phí cho ngành của môn học {monHoc.TenMh} trong học kỳ {activeHocKy.MaHk}.");

                    var existingDangKy = await _context.DangKyHocPhans
                        .AsNoTracking()
                        .FirstOrDefaultAsync(dk => dk.MaSv == createDto.MaSv
                                               && dk.MaMh == maMh
                                               && dk.MaHk == activeHocKy.MaHk);

                    if (existingDangKy != null)
                        throw new Exception($"Sinh viên đã đăng ký môn {monHoc.TenMh} trong học kỳ này.");

                    var dangKy = new DangKyHocPhan
                    {
                        MaSv = createDto.MaSv,
                        MaMh = maMh,
                        MaHk = activeHocKy.MaHk,
                        NgayDk = DateTime.UtcNow,
                        TrangThai = "Đã đăng ký",
                        MaDk = $"DK{nextDkId:D4}"
                    };
                    _context.DangKyHocPhans.Add(dangKy);
                    nextDkId++; 

                    var soTienMonHoc = (monHoc.SoTinChi.Value) * bieuPhi.DonGiaTinChi;
                    chiTietHoaDonList.Add(new ChiTietHoaDonCreateDto
                    {
                        NoiDung = $"Học phí môn: {monHoc.TenMh} ({monHoc.SoTinChi} tín chỉ)",
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
    }
}