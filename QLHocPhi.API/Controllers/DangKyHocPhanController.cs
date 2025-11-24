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
        [Authorize(Roles = "SinhVien")] // Chỉ sinh viên mới cần lấy danh sách này để đăng ký
        public async Task<IActionResult> GetAvailableClasses()
        {
            try
            {
                // Tự động lấy mã SV từ Token
                var maSv = User.FindFirst("MaSv")?.Value;
                if (string.IsNullOrEmpty(maSv)) return Unauthorized();

                var listLop = await _dangKyHocPhanService.GetAvailableClassesForStudentAsync(maSv);
                return Ok(listLop);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpDelete]
        [Authorize(Roles = "PhongTaiChinh,SinhVien")] // Cả 2 đều được
        public async Task<IActionResult> CancelRegistration([FromQuery] string maLhp, [FromQuery] string? maSv)
        {
            try
            {
                // 1. Logic phân quyền lấy MaSv (Giống hệt lúc tạo)
                var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                var tokenMaSv = User.FindFirst("MaSv")?.Value;
                string targetMaSv = maSv;

                if (role == "SinhVien")
                {
                    targetMaSv = tokenMaSv; // Sinh viên chỉ được hủy của mình
                }
                else if (string.IsNullOrEmpty(targetMaSv))
                {
                    return BadRequest("Vui lòng nhập mã sinh viên cần hủy.");
                }

                // 2. Gọi Service
                await _dangKyHocPhanService.CancelRegistrationAsync(targetMaSv, maLhp);

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
    }
}