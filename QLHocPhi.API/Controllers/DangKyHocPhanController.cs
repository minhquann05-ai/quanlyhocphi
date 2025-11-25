using Microsoft.AspNetCore.Mvc;
using QLHocPhi.BLL.Interfaces;
using QLHocPhi.Common.Dtos;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace QLHocPhi.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DangKyHocPhanController : ControllerBase
    {
        private readonly IDangKyHocPhanService _dangKyHocPhanService;

        public DangKyHocPhanController(IDangKyHocPhanService dangKyHocPhanService)
        {
            _dangKyHocPhanService = dangKyHocPhanService;
        }

        [HttpPost]
        [Authorize(Roles = "PhongTaiChinh,SinhVien")]
        public async Task<IActionResult> CreateDangKy([FromBody] DangKyHocPhanCreateDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // 1. LẤY THÔNG TIN TỪ TOKEN
                var role = User.FindFirst(ClaimTypes.Role)?.Value;
                var tokenMaSv = User.FindFirst("MaSv")?.Value;

                // 2. LOGIC PHÂN QUYỀN CHỌN MÃ SV
                if (role == "SinhVien")
                {
                    // Nếu là Sinh viên: BẮT BUỘC dùng mã trong Token (Ghi đè lên input)
                    // Dù SV có nhập MaSv="SV_KHAC" thì cũng bị sửa lại thành mã của chính họ.
                    createDto.MaSv = tokenMaSv;
                }
                else // Nếu là PhongTaiChinh
                {
                    // Nếu là Admin: Bắt buộc phải nhập MaSv trong JSON body
                    if (string.IsNullOrEmpty(createDto.MaSv))
                    {
                        return BadRequest("Vui lòng nhập Mã sinh viên cần đăng ký hộ.");
                    }
                }

                // 3. Kiểm tra cuối cùng
                if (string.IsNullOrEmpty(createDto.MaSv))
                {
                    return Unauthorized("Không xác định được danh tính sinh viên.");
                }

                // 4. Gọi Service
                var generatedHoaDon = await _dangKyHocPhanService.CreateDangKyAsync(createDto);

                return Created($"api/HoaDon/sinhvien/{generatedHoaDon.MaSv}", generatedHoaDon);
            }
            catch (Exception ex)
            {
                // Xử lý lấy InnerException message cho rõ ràng
                var errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return BadRequest(errorMessage);
            }
        }
        [HttpGet("available-classes")]
        [Authorize(Roles = "SinhVien,PhongTaiChinh")] // <--- Mở quyền cho cả Admin
        public async Task<IActionResult> GetAvailableClasses([FromQuery] string? maSv)
        {
            try
            {
                // 1. Lấy thông tin người dùng
                var role = User.FindFirst(ClaimTypes.Role)?.Value;
                var tokenMaSv = User.FindFirst("MaSv")?.Value;
                string targetMaSv = maSv;

                // 2. Phân quyền chọn Mã SV
                if (role == "SinhVien")
                {
                    targetMaSv = tokenMaSv; // Sinh viên bắt buộc dùng mã của mình
                }
                else // Admin
                {
                    if (string.IsNullOrEmpty(targetMaSv))
                    {
                        return BadRequest("Vui lòng nhập Mã sinh viên cần đăng ký.");
                    }
                }

                if (string.IsNullOrEmpty(targetMaSv)) return Unauthorized();

                // 3. Gọi Service
                var listLop = await _dangKyHocPhanService.GetAvailableClassesForStudentAsync(targetMaSv);
                return Ok(listLop);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpDelete]
        [Authorize(Roles = "PhongTaiChinh,SinhVien")]
        public async Task<IActionResult> CancelRegistration([FromQuery] string maLhp, [FromQuery] string? maSv)
        {
            try
            {
                // 1. Lấy Role và Mã SV
                var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                var tokenMaSv = User.FindFirst("MaSv")?.Value;
                string targetMaSv = maSv;

                if (role == "SinhVien")
                {
                    targetMaSv = tokenMaSv;
                }
                else if (string.IsNullOrEmpty(targetMaSv))
                {
                    return BadRequest("Vui lòng nhập mã sinh viên cần hủy.");
                }

                // 2. Gọi Service (Truyền thêm Role)
                await _dangKyHocPhanService.CancelRegistrationAsync(targetMaSv, maLhp, role);

                return Ok(new { message = $"Đã hủy đăng ký lớp {maLhp} thành công." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        /// <summary>
        /// Xem danh sách môn đã đăng ký
        /// - Sinh viên: Tự động lấy của mình
        /// - Admin: Phải truyền ?maSv=...
        /// </summary>
        [HttpGet("registered")]
        [Authorize(Roles = "SinhVien,PhongTaiChinh")] // <--- Cho phép cả Admin
        public async Task<IActionResult> GetRegisteredClasses([FromQuery] string? maSv)
        {
            try
            {
                // 1. Lấy thông tin người dùng
                var role = User.FindFirst(ClaimTypes.Role)?.Value;
                var tokenMaSv = User.FindFirst("MaSv")?.Value;

                string targetMaSv = maSv;

                // 2. Logic phân quyền chọn Mã SV
                if (role == "SinhVien")
                {
                    // Nếu là SV: Bắt buộc dùng mã trong Token
                    targetMaSv = tokenMaSv;
                }
                else if (role == "PhongTaiChinh")
                {
                    // Nếu là Admin: Bắt buộc phải có tham số maSv gửi lên
                    if (string.IsNullOrEmpty(targetMaSv))
                    {
                        return BadRequest("Vui lòng nhập mã sinh viên cần xem kết quả.");
                    }
                }

                if (string.IsNullOrEmpty(targetMaSv)) return Unauthorized();

                // 3. Gọi Service
                var result = await _dangKyHocPhanService.GetRegisteredClassesAsync(targetMaSv);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}