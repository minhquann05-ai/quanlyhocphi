using System.ComponentModel.DataAnnotations;

namespace QLHocPhi.Common.Dtos
{
    public class HoaDonUpdateDto
    {
        [Required]
        public string? TrangThai { get; set; } 
    }
}
