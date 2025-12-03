using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLHocPhi.BLL.Interfaces;
using QLHocPhi.Common.Dtos;

namespace QLHocPhi.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LopHocPhanController : ControllerBase
    {
        private readonly ILopHocPhanService _lopHocPhanService;

        public LopHocPhanController(ILopHocPhanService lopHocPhanService)
        {
            _lopHocPhanService = lopHocPhanService;
        }

        [HttpGet]
        [Authorize(Roles = "PhongTaiChinh,SinhVien")]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _lopHocPhanService.GetAllAsync());
        }

        [HttpGet("nganh/{maNganh}")]
        [Authorize(Roles = "PhongTaiChinh,SinhVien")]
        public async Task<IActionResult> GetByNganh(string maNganh)
        {
            return Ok(await _lopHocPhanService.GetByNganhAsync(maNganh));
        }

        [HttpPost]
        [Authorize(Roles = "PhongTaiChinh")]
        public async Task<IActionResult> Create([FromBody] LopHocPhanCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var result = await _lopHocPhanService.CreateAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{maLhp}")]
        [Authorize(Roles = "PhongTaiChinh")]
        public async Task<IActionResult> Update(string maLhp, [FromBody] LopHocPhanUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                await _lopHocPhanService.UpdateAsync(maLhp, dto);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{maLhp}")]
        [Authorize(Roles = "PhongTaiChinh")]
        public async Task<IActionResult> Delete(string maLhp)
        {
            try
            {
                await _lopHocPhanService.DeleteAsync(maLhp);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpDelete("{maLhp}/students")]
        [Authorize(Roles = "PhongTaiChinh")]
        public async Task<IActionResult> RemoveAllStudents(string maLhp)
        {
            try
            {
                await _lopHocPhanService.RemoveAllStudentsAsync(maLhp);
                return Ok(new { message = $"Đã hủy toàn bộ sinh viên của lớp {maLhp}. Sĩ số đã về 0." });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
