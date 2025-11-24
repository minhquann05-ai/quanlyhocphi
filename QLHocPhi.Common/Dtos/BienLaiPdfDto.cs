using QLHocPhi.Common.Dtos;

namespace QLHocPhi.Common.Dtos
{
    // Lớp này chứa tất cả thông tin cần thiết để in 1 biên lai
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
