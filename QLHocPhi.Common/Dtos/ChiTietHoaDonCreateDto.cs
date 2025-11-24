using System.ComponentModel.DataAnnotations;

namespace QLHocPhi.Common.Dtos
{
    public class ChiTietHoaDonCreateDto
    {
        [Required]
        public string? NoiDung { get; set; }

        [Range(1, double.MaxValue)]
        public decimal SoTien { get; set; }
    }
}