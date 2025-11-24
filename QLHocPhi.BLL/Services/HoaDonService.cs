using AutoMapper;
using Microsoft.EntityFrameworkCore;
using QLHocPhi.BLL.Interfaces;
using QLHocPhi.Common.Dtos;
using QLHocPhi.DAL;
using QLHocPhi.DAL.Entities;

namespace QLHocPhi.BLL.Services
{
    public class HoaDonService : IHoaDonService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public HoaDonService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<HoaDonDto>> GetAllAsync(string? trangThai = null)
        {
            // 1. Khởi tạo query
            var query = _context.HoaDons.AsQueryable();

            // 2. Nếu có truyền trạng thái thì lọc
            if (!string.IsNullOrEmpty(trangThai))
            {
                query = query.Where(hd => hd.TrangThai == trangThai);
            }

            // 3. Include các bảng liên quan và thực thi
            var hoaDons = await query
                .Include(hd => hd.SinhVien)
                .Include(hd => hd.HocKy)
                .Include(hd => hd.ChiTietHoaDons)
                .AsNoTracking()
                .ToListAsync();

            return _mapper.Map<IEnumerable<HoaDonDto>>(hoaDons);
        }

        public async Task<IEnumerable<HoaDonDto>> GetByMaSvAsync(string maSv, string trangThai)
        {
            var query = _context.HoaDons
                .Where(hd => hd.MaSv == maSv);

            if (!string.IsNullOrEmpty(trangThai))
            {
                query = query.Where(hd => hd.TrangThai == trangThai);
            }

            var hoaDons = await query
                .Include(hd => hd.SinhVien)
                .Include(hd => hd.HocKy)
                .Include(hd => hd.ChiTietHoaDons)
                .AsNoTracking()
                .ToListAsync();

            return _mapper.Map<IEnumerable<HoaDonDto>>(hoaDons);
        }

        public async Task<HoaDonDto> CreateAsync(HoaDonCreateDto createDto)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var hoaDon = _mapper.Map<HoaDon>(createDto);

                hoaDon.NgayTao = DateTime.UtcNow;
                hoaDon.TongTien = createDto.ChiTiet.Sum(ct => ct.SoTien);

                var lastHoaDon = await _context.HoaDons.OrderByDescending(hd => hd.MaHd).FirstOrDefaultAsync();
                int nextHdId = 1;
                if (lastHoaDon != null && lastHoaDon.MaHd.StartsWith("HD"))
                {
                    string numberPart = lastHoaDon.MaHd.Substring(2);
                    if (int.TryParse(numberPart, out int lastId))
                        nextHdId = lastId + 1;
                }
                hoaDon.MaHd = $"HD{nextHdId:D4}";

                _context.HoaDons.Add(hoaDon);

                var lastCt = await _context.ChiTietHoaDons
                        .OrderByDescending(ct => ct.MaCt)
                        .AsNoTracking()
                        .FirstOrDefaultAsync();
                int nextCtId = 1;
                if (lastCt != null && lastCt.MaCt.StartsWith("CT"))
                {
                    string numberPart = lastCt.MaCt.Substring(2);
                    if (int.TryParse(numberPart, out int lastId))
                        nextCtId = lastId + 1;
                }
                var chiTietDtos = new List<ChiTietHoaDonDto>();

                foreach (var chiTietDto in createDto.ChiTiet)
                {
                    var chiTiet = _mapper.Map<ChiTietHoaDon>(chiTietDto);
                    chiTiet.MaHd = hoaDon.MaHd; 
                    chiTiet.MaCt = $"CT{nextCtId:D4}"; 

                    _context.ChiTietHoaDons.Add(chiTiet);
                    nextCtId++;
                }

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                var sinhVien = await _context.SinhViens.FindAsync(createDto.MaSv);
                var hocKy = await _context.HocKys.FindAsync(createDto.MaHk);

                return new HoaDonDto
                {
                    MaHd = hoaDon.MaHd,
                    MaSv = hoaDon.MaSv,
                    TenSv = sinhVien?.HoTen,
                    MaHk = hoaDon.MaHk,
                    TenHk = hocKy?.TenHk,
                    NgayTao = hoaDon.NgayTao,
                    TongTien = hoaDon.TongTien,
                    TrangThai = hoaDon.TrangThai,
                    ChiTiet = chiTietDtos 
                };
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw; 
            }
        }

        public async Task UpdateTrangThaiAsync(string maHd, HoaDonUpdateDto updateDto)
        {
            var hoaDon = await _context.HoaDons.FindAsync(maHd);
            if (hoaDon == null)
                throw new KeyNotFoundException("Không tìm thấy hóa đơn");

            hoaDon.TrangThai = updateDto.TrangThai;
            _context.HoaDons.Update(hoaDon);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string maHd)
        {
            var hoaDon = await _context.HoaDons.FindAsync(maHd);
            if (hoaDon == null)
                throw new KeyNotFoundException("Không tìm thấy hóa đơn");

            _context.HoaDons.Remove(hoaDon);
            await _context.SaveChangesAsync();
        }
    }
}
