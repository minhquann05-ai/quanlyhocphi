using QLHocPhi.Common.Dtos;

namespace QLHocPhi.Common.Dtos
{
    public class BienLaiPdfDto
    {
        public string? SoBienLai { get; set; }
        public DateTime NgayIn { get; set; }

        public string? MaSv { get; set; }
        public string? HoTenSv { get; set; }
        public string? TenLop { get; set; }

        public decimal SoTienThanhToan { get; set; }
        public string? PhuongThucThanhToan { get; set; }
        public string? TenHocKy { get; set; }

        public List<ChiTietHoaDonDto>? ChiTietThanhToan { get; set; }
    }
}
