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
    public class HoaDonController : ControllerBase
    {
        private readonly IHoaDonService _hoaDonService;

        public HoaDonController(IHoaDonService hoaDonService)
        {
            _hoaDonService = hoaDonService;
        }

        [HttpGet]
        [Authorize(Roles = "PhongTaiChinh")]
        public async Task<IActionResult> GetAllHoaDon([FromQuery] string? trangThai)
        {
            // Truyền tham số trangThai vào Service
            var result = await _hoaDonService.GetAllAsync(trangThai);
            return Ok(result);
        }

        [HttpGet("sinhvien")]
        [Authorize(Roles = "PhongTaiChinh,SinhVien")]
        public async Task<IActionResult> GetHoaDonByMaSv([FromQuery] string? maSv, [FromQuery] string? trangThai)
        {
            // 1. Lấy thông tin từ Token
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var tokenMaSv = User.FindFirst("MaSv")?.Value;

            string targetMaSv = maSv;

            // 2. Logic phân quyền chọn mã SV
            if (role == "SinhVien")
            {
                // Sinh viên chỉ được xem của chính mình
                targetMaSv = tokenMaSv;
            }
            else // PhongTaiChinh
            {
                if (string.IsNullOrEmpty(targetMaSv))
                {
                    return BadRequest("Vui lòng nhập mã sinh viên cần tra cứu.");
                }
            }

            if (string.IsNullOrEmpty(targetMaSv))
            {
                return Unauthorized("Không xác định được danh tính sinh viên.");
            }

            // 3. Gọi Service
            var result = await _hoaDonService.GetByMaSvAsync(targetMaSv, trangThai);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "PhongTaiChinh")]
        public async Task<IActionResult> CreateHoaDon([FromBody] HoaDonCreateDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var newHoaDon = await _hoaDonService.CreateAsync(createDto);
                return Created($"api/HoaDon/sinhvien/{newHoaDon.MaSv}", newHoaDon);
            }
            catch (Exception ex)
            {
                var errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return BadRequest(errorMessage);
            }
        }

        [HttpPut("{maHd}")]
        [Authorize(Roles = "PhongTaiChinh")]
        public async Task<IActionResult> UpdateHoaDonTrangThai(string maHd, [FromBody] HoaDonUpdateDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _hoaDonService.UpdateTrangThaiAsync(maHd, updateDto);
                return NoContent(); 
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

        [HttpDelete("{maHd}")]
        [Authorize(Roles = "PhongTaiChinh")]
        public async Task<IActionResult> DeleteHoaDon(string maHd)
        {
            try
            {
                await _hoaDonService.DeleteAsync(maHd);
                return NoContent(); 
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
