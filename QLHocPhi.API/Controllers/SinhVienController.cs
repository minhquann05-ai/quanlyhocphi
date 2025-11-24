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

        [HttpGet]
        [Authorize(Roles = "PhongTaiChinh")]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _sinhVienService.GetAllAsync());
        }

        [HttpGet("sinhvien")]
        [Authorize(Roles = "PhongTaiChinh,SinhVien")]
        public async Task<IActionResult> GetById([FromQuery] string? maSv)
        {
            // 1. Lấy thông tin từ Token
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var tokenMaSv = User.FindFirst("MaSv")?.Value;

            string targetMaSv = maSv;

            // 2. Logic phân quyền chọn mã SV
            if (role == "SinhVien")
            {
                // Nếu là Sinh viên: BẮT BUỘC dùng mã trong Token
                targetMaSv = tokenMaSv;
            }
            else // PhongTaiChinh
            {
                // Nếu là Admin: Bắt buộc phải nhập MaSv
                if (string.IsNullOrEmpty(targetMaSv))
                {
                    return BadRequest("Vui lòng nhập Mã sinh viên cần tra cứu.");
                }
            }

            if (string.IsNullOrEmpty(targetMaSv))
            {
                return Unauthorized("Không xác định được danh tính sinh viên.");
            }

            // 3. Gọi Service
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
    }
}
