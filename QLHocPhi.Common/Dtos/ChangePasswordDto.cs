using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLHocPhi.Common.Dtos
{
    public class ChangePasswordDto
    {
        [Required]
        public string? TenDangNhap { get; set; } // Mã sinh viên

        [Required]
        public string? MatKhauCu { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự")]
        public string? MatKhauMoi { get; set; }

        [Compare("MatKhauMoi", ErrorMessage = "Mật khẩu nhập lại không khớp")]
        public string? NhapLaiMatKhauMoi { get; set; }
    }
}
