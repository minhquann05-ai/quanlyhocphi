using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLHocPhi.BLL.Interfaces;
using QLHocPhi.Common.Dtos;
using QLHocPhi.DAL.Entities;
using System.Security.Claims;

namespace QLHocPhi.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SinhVienController : ControllerBase
    {
        private readonly ISinhVienService _sinhVienService;

        public SinhVienController(ISinhVienService sinhVienService)
        {
            _sinhVienService = sinhVienService;
        }

        [HttpPost("search")]
        [Authorize(Roles = "PhongTaiChinh")]
        public async Task<IActionResult> SearchSinhVien([FromBody] SinhVienSearchDto searchDto)
        {
            var result = await _sinhVienService.GetAllAsync(searchDto);
            return Ok(result);
        }
        [HttpGet]
        [Authorize(Roles = "PhongTaiChinh")]
        public async Task<IActionResult> GetAllSinhVien([FromQuery] SinhVienSearchDto searchDto)
        {
            var result = await _sinhVienService.GetAllAsync(searchDto);
            return Ok(result);
        }

        [HttpGet("sinhvien")]
        [Authorize(Roles = "PhongTaiChinh,SinhVien")]
        public async Task<IActionResult> GetById([FromQuery] string? maSv)
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
                    return BadRequest("Vui lòng nhập Mã sinh viên cần tra cứu.");
                }
            }

            if (string.IsNullOrEmpty(targetMaSv))
            {
                return Unauthorized("Không xác định được danh tính sinh viên.");
            }

            try
            {
                var result = await _sinhVienService.GetByIdAsync(targetMaSv);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost]
        [Authorize(Roles = "PhongTaiChinh")]
        public async Task<IActionResult> Create([FromBody] SinhVienCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var result = await _sinhVienService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { maSv = result.MaSv }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPut("{maSv}")]
        [Authorize(Roles = "PhongTaiChinh")]   
        public async Task<IActionResult> UpdateSinhVien(string maSv, [FromBody] SinhVienUpdateDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _sinhVienService.UpdateAsync(maSv, updateDto);
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

        [HttpDelete("{maSv}")]
        [Authorize(Roles = "PhongTaiChinh")]   
        public async Task<IActionResult> DeleteSinhVien(string maSv)
        {
            try
            {
                await _sinhVienService.DeleteAsync(maSv);
                return NoContent();    
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)           
            {
                return BadRequest($"Không thể xóa sinh viên này (có thể do đã có dữ liệu học phí). Chi tiết: {ex.Message}");
            }
        }
    }
}
