using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLHocPhi.BLL.Interfaces;
using QLHocPhi.Common.Dtos;
using QLHocPhi.DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace QLHocPhi.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NguoiDungController : ControllerBase
    {
        private readonly INguoiDungService _nguoiDungService;

        public NguoiDungController(INguoiDungService nguoiDungService)
        {
            _nguoiDungService = nguoiDungService;
        }

        /// <summary>
        /// Đăng nhập hệ thống
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var user = await _nguoiDungService.LoginAsync(loginDto);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Đổi mật khẩu (Dành cho sinh viên)
        /// </summary>
        [HttpPut("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // --- NÂNG CẤP BẢO MẬT ---
                // Lấy tên đăng nhập từ chính Token của người đang gọi API
                // Tránh trường hợp ông A đăng nhập nhưng cố tình nhập User của ông B để đổi
                var currentUserName = User.FindFirst(ClaimTypes.Name)?.Value;

                if (string.IsNullOrEmpty(currentUserName))
                    return Unauthorized();

                // Ghi đè tên đăng nhập trong DTO bằng tên thật trong Token
                dto.TenDangNhap = currentUserName;
                // ------------------------

                await _nguoiDungService.ChangePasswordAsync(dto);
                return Ok(new { message = "Đổi mật khẩu thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// [Admin] Tự động tạo tài khoản cho tất cả sinh viên chưa có (Pass: 123456)
        /// </summary>
        [HttpPost("generate-student-accounts")]
        [Authorize(Roles = "PhongTaiChinh")]
        public async Task<IActionResult> GenerateStudentAccounts()
        {
            try
            {
                int count = await _nguoiDungService.GenerateAccountsForStudentsAsync();
                return Ok(new { message = $"Đã tạo thành công {count} tài khoản sinh viên mới." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("create-admin-default")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateDefaultAdmin()
        {
            try
            {
                // Gọi Service xử lý (Thay vì dùng _context)
                await _nguoiDungService.CreateDefaultAdminAsync();
                return Ok("Đã tạo tài khoản: admin / 123456");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
