using System.ComponentModel.DataAnnotations;

namespace QLHocPhi.Common.Dtos
{
    public class DangKyHocPhanCreateDto
    {
        public string? MaSv { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Phải đăng ký ít nhất 1 môn học")]
        public List<string> ListMaLhp { get; set; }
    }
}
