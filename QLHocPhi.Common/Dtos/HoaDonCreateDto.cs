using QLHocPhi.Common.Dtos;
using System.ComponentModel.DataAnnotations;

namespace QLHocPhi.Common.Dtos
{
    public class HoaDonCreateDto
    {
        [Required]
        public string? MaSv { get; set; }

        [Required]
        public string? MaHk { get; set; }

        public string TrangThai { get; set; } = "Chưa thanh toán";

        [MinLength(1, ErrorMessage = "Hóa đơn phải có ít nhất 1 chi tiết")]
        public List<ChiTietHoaDonCreateDto>? ChiTiet { get; set; }
    }
}
