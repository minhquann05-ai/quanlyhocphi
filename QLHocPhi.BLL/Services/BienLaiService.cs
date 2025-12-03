using AutoMapper;
using Microsoft.EntityFrameworkCore;
using QLHocPhi.BLL.Interfaces;
using QLHocPhi.BLL.PdfTemplates; 
using QLHocPhi.Common.Dtos;
using QLHocPhi.DAL;
using QLHocPhi.DAL.Entities;
using QuestPDF.Fluent; 

namespace QuanLyHocPhi.BLL.Services
{
    public class BienLaiService : IBienLaiService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public BienLaiService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<byte[]> GenerateBienLaiPdfAsync(string maHd)
        {
            var bienLai = await _context.BienLais
                .Include(bl => bl.ThanhToan)
                    .ThenInclude(tt => tt.HoaDon)
                        .ThenInclude(hd => hd.SinhVien)
                            .ThenInclude(sv => sv.LopHoc)
                .Include(bl => bl.ThanhToan.HoaDon.HocKy)
                .Include(bl => bl.ThanhToan.HoaDon.ChiTietHoaDons)
                .AsNoTracking()
                .FirstOrDefaultAsync(bl => bl.ThanhToan.MaHd == maHd);

            if (bienLai == null)
                throw new KeyNotFoundException("Không tìm thấy biên lai cho hóa đơn này. Hóa đơn có thể chưa được thanh toán.");

            var model = new BienLaiPdfDto
            {
                SoBienLai = bienLai.SoBienLai,
                NgayIn = bienLai.NgayIn ?? DateTime.Now,
                MaSv = bienLai.ThanhToan.HoaDon.SinhVien.MaSv,
                HoTenSv = bienLai.ThanhToan.HoaDon.SinhVien.HoTen,
                TenLop = bienLai.ThanhToan.HoaDon.SinhVien.LopHoc?.TenLop,
                SoTienThanhToan = bienLai.ThanhToan.SoTienTt ?? 0,
                PhuongThucThanhToan = bienLai.ThanhToan.PhuongThuc,
                TenHocKy = bienLai.ThanhToan.HoaDon.HocKy.TenHk,

                ChiTietThanhToan = _mapper.Map<List<ChiTietHoaDonDto>>(bienLai.ThanhToan.HoaDon.ChiTietHoaDons)
            };

            var document = new BienLaiTemplate(model);


            byte[] pdfBytes = document.GeneratePdf();

            return pdfBytes;
        }

        public async Task<IEnumerable<BienLaiDto>> GetAllAsync()
        {
            var list = await _context.BienLais
                .Include(bl => bl.ThanhToan.HoaDon)
                .ThenInclude(hd => hd.SinhVien)    
                .AsNoTracking()
                .ToListAsync();
            return _mapper.Map<IEnumerable<BienLaiDto>>(list);
        }

        public async Task<IEnumerable<BienLaiDto>> GetByMaSvAsync(string maSv)
        {
            var list = await _context.BienLais
                .Where(bl => bl.ThanhToan.HoaDon.MaSv == maSv)    
                .Include(bl => bl.ThanhToan.HoaDon)
                .ThenInclude(hd => hd.SinhVien)
                .AsNoTracking()
                .ToListAsync();
            return _mapper.Map<IEnumerable<BienLaiDto>>(list);
        }
        public async Task<int> SyncMissingBienLaiAsync()
        {
            int count = 0;

            var paidInvoices = await _context.HoaDons
                .Where(hd => hd.TrangThai == "Đã thanh toán")
                .Include(hd => hd.SinhVien)        
                .ToListAsync();

            var lastTt = await _context.ThanhToans.OrderByDescending(x => x.MaTt).FirstOrDefaultAsync();
            var lastBl = await _context.BienLais.OrderByDescending(x => x.MaBl).FirstOrDefaultAsync();

            int nextTtId = 1;
            if (lastTt != null && int.TryParse(lastTt.MaTt.Substring(2), out int idTt)) nextTtId = idTt + 1;

            int nextBlId = 1;
            if (lastBl != null && int.TryParse(lastBl.MaBl.Substring(2), out int idBl)) nextBlId = idBl + 1;

            foreach (var hd in paidInvoices)
            {
                var thanhToan = await _context.ThanhToans.FirstOrDefaultAsync(tt => tt.MaHd == hd.MaHd);

                if (thanhToan == null)
                {
                    thanhToan = new ThanhToan
                    {
                        MaTt = $"TT{nextTtId:D4}",
                        MaHd = hd.MaHd,
                        NgayTt = hd.NgayTao ?? DateTime.UtcNow,        
                        SoTienTt = hd.TongTien,
                        PhuongThuc = "Tiền mặt (Bổ sung)",        
                        TrangThaiTt = "Thành công"
                    };
                    _context.ThanhToans.Add(thanhToan);
                    nextTtId++;
                }

                var bienLai = await _context.BienLais.FirstOrDefaultAsync(bl => bl.MaTt == thanhToan.MaTt);

                if (bienLai == null)
                {
                    var newBienLai = new BienLai
                    {
                        MaBl = $"BL{nextBlId:D4}",
                        MaTt = thanhToan.MaTt,
                        SoBienLai = $"SBL_AUTO_{DateTime.Now:yyyyMMdd}_{nextBlId}",
                        NgayIn = DateTime.UtcNow,
                        NoiDung = $"Biên lai bổ sung cho HĐ {hd.MaHd}"
                    };
                    _context.BienLais.Add(newBienLai);
                    nextBlId++;
                    count++;
                }
            }

            await _context.SaveChangesAsync();
            return count;
        }
    }
}
