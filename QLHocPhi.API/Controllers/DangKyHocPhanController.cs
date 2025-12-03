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
                var role = User.FindFirst(ClaimTypes.Role)?.Value;
                var tokenMaSv = User.FindFirst("MaSv")?.Value;

                if (role == "SinhVien")
                {
                    createDto.MaSv = tokenMaSv;
                }
                else    
                {
                    if (string.IsNullOrEmpty(createDto.MaSv))
                    {
                        return BadRequest("Vui lòng nhập Mã sinh viên cần đăng ký hộ.");
                    }
                }

                if (string.IsNullOrEmpty(createDto.MaSv))
                {
                    return Unauthorized("Không xác định được danh tính sinh viên.");
                }

                var generatedHoaDon = await _dangKyHocPhanService.CreateDangKyAsync(createDto);

                return Created($"api/HoaDon/sinhvien/{generatedHoaDon.MaSv}", generatedHoaDon);
            }
            catch (Exception ex)
            {
                var errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return BadRequest(errorMessage);
            }
        }
        [HttpGet("available-classes")]
        [Authorize(Roles = "SinhVien,PhongTaiChinh")]       
        public async Task<IActionResult> GetAvailableClasses([FromQuery] string? maSv)
        {
            try
            {
                var role = User.FindFirst(ClaimTypes.Role)?.Value;
                var tokenMaSv = User.FindFirst("MaSv")?.Value;
                string targetMaSv = maSv;

                if (role == "SinhVien")
                {
                    targetMaSv = tokenMaSv;         
                }
                else  
                {
                    if (string.IsNullOrEmpty(targetMaSv))
                    {
                        return BadRequest("Vui lòng nhập Mã sinh viên cần đăng ký.");
                    }
                }

                if (string.IsNullOrEmpty(targetMaSv)) return Unauthorized();

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
        [HttpGet("registered")]
        [Authorize(Roles = "SinhVien,PhongTaiChinh")]      
        public async Task<IActionResult> GetRegisteredClasses([FromQuery] string? maSv)
        {
            try
            {
                var role = User.FindFirst(ClaimTypes.Role)?.Value;
                var tokenMaSv = User.FindFirst("MaSv")?.Value;

                string targetMaSv = maSv;

                if (role == "SinhVien")
                {
                    targetMaSv = tokenMaSv;
                }
                else if (role == "PhongTaiChinh")
                {
                    if (string.IsNullOrEmpty(targetMaSv))
                    {
                        return BadRequest("Vui lòng nhập mã sinh viên cần xem kết quả.");
                    }
                }

                if (string.IsNullOrEmpty(targetMaSv)) return Unauthorized();

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