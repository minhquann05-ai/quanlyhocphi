using System.ComponentModel.DataAnnotations;

namespace QLHocPhi.Common.Dtos
{
    public class ThanhToanCreateDto
    {
        public string? MaSv { get; set; }
        [Required]
        public string? MaHd { get; set; }

        [Required]
        public string? PhuongThuc { get; set; } // "Online", "Tiền mặt", "Chuyển khoản"

        [Range(1, double.MaxValue)]
        public decimal SoTienTt { get; set; } 
    }
}
