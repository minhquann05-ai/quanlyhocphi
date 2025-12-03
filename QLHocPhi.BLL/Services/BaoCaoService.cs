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

        public async Task<List<BaoCaoCongNoDto>> GetListBaoCaoAsync(string maHk, string? maSv)
        {
            var query = _context.HoaDons
                .Where(hd => hd.TrangThai == "Chưa thanh toán")
                .Include(hd => hd.SinhVien)
                    .ThenInclude(sv => sv.LopHoc)
                .AsQueryable();            

            if (!string.IsNullOrEmpty(maHk))
            {
                if (maHk.StartsWith("HK"))
                {
                    query = query.Where(hd => hd.MaHk == maHk);
                }
                else
                {
                    query = query.Where(hd => hd.MaHk.Contains(maHk));
                }
            }

            if (!string.IsNullOrEmpty(maSv))
            {
                query = query.Where(hd => hd.MaSv.Contains(maSv));
            }

            query = query.OrderBy(hd => hd.SinhVien.MaLop).ThenBy(hd => hd.MaSv);

            var listData = await query.AsNoTracking().ToListAsync();

            var result = listData.Select((hd, index) => new BaoCaoCongNoDto
            {
                Stt = index + 1,
                MaSv = hd.MaSv,
                HoTen = hd.SinhVien?.HoTen ?? "N/A",
                TenLop = hd.SinhVien?.LopHoc?.TenLop ?? "N/A",
                MaHd = hd.MaHd,
                SoTienNo = hd.TongTien ?? 0
            }).ToList();

            return result;
        }

        public async Task<byte[]> ExportBaoCaoCongNoPdfAsync(string maHk)
        {
            var hocKy = await _context.HocKys.FindAsync(maHk);
            string tenHocKy = hocKy?.TenHk ?? maHk;          

            var dataReport = await GetListBaoCaoAsync(maHk, null);

            var document = new BaoCaoCongNoTemplate(dataReport, tenHocKy);
            return document.GeneratePdf();
        }
    }
}