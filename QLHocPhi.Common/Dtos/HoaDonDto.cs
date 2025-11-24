using QLHocPhi.Common.Dtos;

namespace QLHocPhi.Common.Dtos
{
    public class HoaDonDto
    {
        public string? MaHd { get; set; }
        public string? MaSv { get; set; }
        public string? TenSv { get; set; } 
        public string? MaHk { get; set; }
        public string? TenHk { get; set; } 
        public DateTime? NgayTao { get; set; }
        public decimal? TongTien { get; set; }
        public string? TrangThai { get; set; }

        // Danh sách chi tiết
        public List<ChiTietHoaDonDto>? ChiTiet { get; set; }
    }
}
