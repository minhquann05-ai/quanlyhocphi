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

        // --- NHÓM XEM (Cả 2 quyền) ---

        /// <summary>
        /// Xem tất cả lớp học phần
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "PhongTaiChinh,SinhVien")]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _lopHocPhanService.GetAllAsync());
        }

        /// <summary>
        /// Xem lớp học phần theo Ngành (VD: CNTT)
        /// </summary>
        [HttpGet("nganh/{maNganh}")]
        [Authorize(Roles = "PhongTaiChinh,SinhVien")]
        public async Task<IActionResult> GetByNganh(string maNganh)
        {
            return Ok(await _lopHocPhanService.GetByNganhAsync(maNganh));
        }

        // --- NHÓM QUẢN LÝ (Chỉ Phòng Tài Chính) ---

        /// <summary>
        /// Thêm lớp học phần mới
        /// </summary>
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

        /// <summary>
        /// Sửa thông tin lớp (Tên, Sĩ số max, Trạng thái)
        /// </summary>
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

        /// <summary>
        /// Xóa lớp học phần (Chỉ xóa được khi chưa có ai đăng ký)
        /// </summary>
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
    }
}
