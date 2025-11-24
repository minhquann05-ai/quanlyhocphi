using QLHocPhi.BLL.Interfaces;
using QLHocPhi.BLL.PdfTemplates;
using QLHocPhi.Common.Dtos;
using QLHocPhi.DAL;
using Microsoft.EntityFrameworkCore; 
using QuestPDF.Fluent;              
using QuestPDF.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLHocPhi.BLL.Services
{
    public class BaoCaoService : IBaoCaoService
    {
        private readonly AppDbContext _context;

        public BaoCaoService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> ExportBaoCaoCongNoPdfAsync(string maHk)
        {
            // 1. Lấy tên học kỳ
            var hocKy = await _context.HocKys.FindAsync(maHk);
            if (hocKy == null) throw new Exception("Học kỳ không tồn tại");

            // 2. Truy vấn dữ liệu: Lấy hóa đơn chưa thanh toán của học kỳ đó
            var query = await _context.HoaDons
                .Where(hd => hd.MaHk == maHk && hd.TrangThai == "Chưa thanh toán")
                .Include(hd => hd.SinhVien)
                    .ThenInclude(sv => sv.LopHoc)
                .OrderBy(hd => hd.SinhVien.MaLop) // Sắp xếp theo lớp
                .AsNoTracking()
                .ToListAsync();

            // 3. Chuyển sang DTO cho báo cáo
            var dataReport = query.Select((hd, index) => new BaoCaoCongNoDto
            {
                Stt = index + 1,
                MaSv = hd.MaSv,
                HoTen = hd.SinhVien?.HoTen ?? "N/A",
                TenLop = hd.SinhVien?.LopHoc?.TenLop ?? "N/A",
                MaHd = hd.MaHd,
                SoTienNo = hd.TongTien ?? 0
            }).ToList();

            // 4. Tạo PDF
            var document = new BaoCaoCongNoTemplate(dataReport, hocKy.TenHk);
            return document.GeneratePdf();
        }
    }
}
