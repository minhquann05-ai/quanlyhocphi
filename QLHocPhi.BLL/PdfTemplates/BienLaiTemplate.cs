using QLHocPhi.Common.Dtos;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace QLHocPhi.BLL.PdfTemplates
{
    public class BienLaiTemplate : IDocument
    {
        private readonly BienLaiPdfDto _model;

        public BienLaiTemplate(BienLaiPdfDto model)
        {
            _model = model;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container
                .Page(page =>
                {
                    page.Margin(50);
                    page.Size(PageSizes.A4);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12).FontFamily("Arial")); 

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
                    col.Item().Text("TRƯỜNG ĐẠI HỌC MỞ THÀNH PHỐ HỒ CHÍ MINH").SemiBold().FontSize(14);
                    col.Item().Text("Phòng Tài chính - Kế toán");
                    col.Item().Text("Website: ou.edu.vn");
                });
                

                row.ConstantItem(150).Column(col =>
                {
                    col.Item().Text("BIÊN LAI THU HỌC PHÍ").Bold().FontSize(16).FontColor(Colors.Red.Medium);
                    col.Item().Text($"Số: {_model.SoBienLai}").Bold();
                    col.Item().Text($"Ngày: {_model.NgayIn:dd/MM/yyyy}");
                });
            });
        }

        void ComposeContent(IContainer container)
        {
            container.PaddingVertical(40).Column(col =>
            {
                col.Spacing(20);

                col.Item().Column(colSv =>
                {
                    colSv.Item().Text("THÔNG TIN SINH VIÊN").Bold().Underline();
                    colSv.Item().Text($"Họ và tên: {_model.HoTenSv}");
                    colSv.Item().Text($"Mã sinh viên: {_model.MaSv}");
                    colSv.Item().Text($"Lớp: {_model.TenLop ?? "N/A"}");
                    colSv.Item().Text($"Học kỳ thanh toán: {_model.TenHocKy}");
                });

                col.Item().Element(ComposeTable);

                col.Item().AlignRight().Text($"Tổng tiền thanh toán: {string.Format("{0:N0}", _model.SoTienThanhToan)} VNĐ")
                    .Bold().FontSize(14);

                col.Item().Text($"Hình thức thanh toán: {_model.PhuongThucThanhToan}");

                col.Item().PaddingTop(50).Row(row =>
                {
                    row.RelativeItem().AlignCenter().Text("Người nộp tiền\n(Ký, ghi rõ họ tên)");
                    row.RelativeItem().AlignCenter().Text("Người thu tiền\n(Ký, ghi rõ họ tên)");
                });
            });
        }

        void ComposeTable(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3); 
                    columns.RelativeColumn(1); 
                });

                table.Header(header =>
                {
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Nội dung").Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text("Số tiền (VNĐ)").Bold();
                });

                foreach (var item in _model.ChiTietThanhToan)
                {
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(item.NoiDung);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text(string.Format("{0:N0}", item.SoTien));
                }
            });
        }

        void ComposeFooter(IContainer container)
        {
            container.AlignCenter().Text(text =>
            {
                text.Span("Trang ");
                text.CurrentPageNumber();
                text.Span(" / ");
                text.TotalPages();
            });
        }
    }
}