using QLHocPhi.Common.Dtos;
using QuestPDF.Infrastructure;
using QuestPDF.Fluent;      
using QuestPDF.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLHocPhi.BLL.PdfTemplates
{
    public class BaoCaoCongNoTemplate : IDocument
    {
        private readonly List<BaoCaoCongNoDto> _data;
        private readonly string _tenHocKy;

        public BaoCaoCongNoTemplate(List<BaoCaoCongNoDto> data, string tenHocKy)
        {
            _data = data;
            _tenHocKy = tenHocKy;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Element(ComposeFooter);
            });
        }

        void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("TRƯỜNG ĐẠI HỌC MỞ THÀNH PHỐ HỒ CHÍ MINH").Bold();
                    col.Item().Text("Phòng Tài chính - Kế toán");
                    col.Item().Text("Website: ou.edu.vn");
                });

                row.RelativeItem().Column(col =>
                {
                    col.Item().AlignRight().Text("CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM").Bold();
                    col.Item().AlignRight().Text("Độc lập - Tự do - Hạnh phúc").Italic();
                });
            });
        }

        void ComposeContent(IContainer container)
        {
            container.PaddingVertical(20).Column(col =>
            {
                col.Item().AlignCenter().Text($"BÁO CÁO CÔNG NỢ SINH VIÊN - {_tenHocKy.ToUpper()}")
                    .Bold().FontSize(16).FontColor(Colors.Blue.Medium);

                col.Item().PaddingTop(10).Text($"Ngày xuất báo cáo: {DateTime.Now:dd/MM/yyyy}");

                col.Item().PaddingTop(10).Element(ComposeTable);

                col.Item().PaddingTop(10).AlignRight().Text($"Tổng số sinh viên nợ: {_data.Count}").Bold();
                col.Item().AlignRight().Text($"Tổng tiền nợ: {string.Format("{0:N0}", _data.Sum(x => x.SoTienNo))} VNĐ").Bold().FontSize(12);
            });
        }

        void ComposeTable(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(40);  // STT
                    columns.ConstantColumn(80);  // Mã SV
                    columns.RelativeColumn();    // Họ tên
                    columns.ConstantColumn(80);  // Lớp
                    columns.ConstantColumn(80);  // Mã HĐ
                    columns.ConstantColumn(100); // Số tiền
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).Text("STT");
                    header.Cell().Element(CellStyle).Text("Mã SV");
                    header.Cell().Element(CellStyle).Text("Họ và Tên");
                    header.Cell().Element(CellStyle).Text("Lớp");
                    header.Cell().Element(CellStyle).Text("Mã HĐ");
                    header.Cell().Element(CellStyle).AlignRight().Text("Số tiền nợ");

                    static IContainer CellStyle(IContainer container)
                    {
                        return container.Background(Colors.Grey.Lighten3).Padding(5).Border(1).BorderColor(Colors.Grey.Medium).DefaultTextStyle(x => x.Bold());
                    }
                });

                // Rows
                foreach (var item in _data)
                {
                    table.Cell().Element(CellStyle).Text(item.Stt.ToString());
                    table.Cell().Element(CellStyle).Text(item.MaSv);
                    table.Cell().Element(CellStyle).Text(item.HoTen);
                    table.Cell().Element(CellStyle).Text(item.TenLop);
                    table.Cell().Element(CellStyle).Text(item.MaHd);
                    table.Cell().Element(CellStyle).AlignRight().Text(string.Format("{0:N0}", item.SoTienNo));

                    static IContainer CellStyle(IContainer container)
                    {
                        return container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5);
                    }
                }
            });
        }

        void ComposeFooter(IContainer container)
        {
            container.AlignCenter().Text(x =>
            {
                x.CurrentPageNumber();
                x.Span(" / ");
                x.TotalPages();
            });
        }
    }
}
