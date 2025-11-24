using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLHocPhi.Common.Dtos
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
        public string? TenDangNhap { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        public string? MatKhau { get; set; }
    }
}
