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
    public class ThanhToanController : ControllerBase
    {
        private readonly IThanhToanService _thanhToanService;

        public ThanhToanController(IThanhToanService thanhToanService)
        {
            _thanhToanService = thanhToanService;
        }

        [HttpPost]
        [Authorize(Roles = "PhongTaiChinh,SinhVien")]
        public async Task<IActionResult> CreateThanhToan([FromBody] ThanhToanCreateDto createDto)
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
                else if (role == "PhongTaiChinh")
                {
                    if (string.IsNullOrEmpty(createDto.MaSv))
                    {
                        return BadRequest("Vui lòng nhập Mã sinh viên chủ hóa đơn để xác nhận.");
                    }
                }

                var generatedBienLai = await _thanhToanService.CreateThanhToanAsync(createDto);

                return CreatedAtAction("DownloadBienLai", "BienLai", new { maHd = createDto.MaHd }, generatedBienLai);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
