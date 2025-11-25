using AutoMapper;
using Microsoft.EntityFrameworkCore;
using QLHocPhi.BLL.Interfaces;
using QLHocPhi.Common.Dtos;
using QLHocPhi.DAL;
using QLHocPhi.DAL.Entities;

namespace QuanLyHocPhi.BLL.Services
{
    public class ThanhToanService : IThanhToanService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ThanhToanService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<BienLaiDto> CreateThanhToanAsync(ThanhToanCreateDto createDto)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var hoaDon = await _context.HoaDons
                    .Include(hd => hd.SinhVien) 
                    .FirstOrDefaultAsync(hd => hd.MaHd == createDto.MaHd);

                if (string.IsNullOrEmpty(createDto.MaSv))
                {
                    throw new Exception("Thông tin sinh viên thanh toán không được để trống.");
                }
                if (hoaDon.MaSv != createDto.MaSv)
                {
                    // Nếu mã SV không khớp với chủ nhân hóa đơn -> Báo lỗi
                    throw new Exception($"Hóa đơn {hoaDon.MaHd} không thuộc về sinh viên {createDto.MaSv}. Vui lòng kiểm tra lại.");
                }

                if (hoaDon == null)
                    throw new KeyNotFoundException("Hóa đơn không tồn tại.");

                if (hoaDon.TrangThai == "Đã thanh toán")
                    throw new Exception("Hóa đơn này đã được thanh toán trước đó.");

                if (hoaDon.TongTien != createDto.SoTienTt)
                    throw new Exception("Số tiền thanh toán không khớp với tổng tiền của hóa đơn.");
                if (hoaDon.NgayTao.HasValue)
                {
                    hoaDon.NgayTao = DateTime.SpecifyKind(hoaDon.NgayTao.Value, DateTimeKind.Utc);
                }

                var thanhToan = _mapper.Map<ThanhToan>(createDto);
                thanhToan.NgayTt = DateTime.UtcNow;
                thanhToan.TrangThaiTt = "Thành công";

                var lastTt = await _context.ThanhToans.OrderByDescending(tt => tt.MaTt).FirstOrDefaultAsync();
                int nextTtId = 1;
                if (lastTt != null && lastTt.MaTt.StartsWith("TT"))
                {
                    if (int.TryParse(lastTt.MaTt.Substring(2), out int lastId))
                        nextTtId = lastId + 1;
                }
                thanhToan.MaTt = $"TT{nextTtId:D4}";

                _context.ThanhToans.Add(thanhToan);

                var bienLai = new BienLai
                {
                    MaTt = thanhToan.MaTt,
                    SoBienLai = $"SBL{DateTime.UtcNow:yyyyMMddHHmmss}", 
                    NgayIn = DateTime.UtcNow,
                    NoiDung = $"Thanh toán học phí cho hóa đơn {hoaDon.MaHd} - SV: {hoaDon.SinhVien?.HoTen}"
                };

                var lastBl = await _context.BienLais.OrderByDescending(bl => bl.MaBl).FirstOrDefaultAsync();
                int nextBlId = 1;
                if (lastBl != null && lastBl.MaBl.StartsWith("BL"))
                {
                    if (int.TryParse(lastBl.MaBl.Substring(2), out int lastId))
                        nextBlId = lastId + 1;
                }
                bienLai.MaBl = $"BL{nextBlId:D4}";

                _context.BienLais.Add(bienLai);

                hoaDon.TrangThai = "Đã thanh toán";
                _context.HoaDons.Update(hoaDon);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                await _context.Entry(bienLai).Reference(b => b.ThanhToan).LoadAsync();
                await _context.Entry(thanhToan).Reference(t => t.HoaDon).LoadAsync();

                return _mapper.Map<BienLaiDto>(bienLai);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(); 
                throw;
            }
        }
    }
}
