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
                // 1. Lấy thông tin User
                var role = User.FindFirst(ClaimTypes.Role)?.Value;
                var tokenMaSv = User.FindFirst("MaSv")?.Value;

                // 2. Phân quyền điền Mã SV
                if (role == "SinhVien")
                {
                    // Nếu là Sinh viên: Ghi đè MaSv trong DTO bằng mã trong Token
                    createDto.MaSv = tokenMaSv;
                }
                else if (role == "PhongTaiChinh")
                {
                    // Nếu là Admin: Bắt buộc phải nhập MaSv
                    if (string.IsNullOrEmpty(createDto.MaSv))
                    {
                        return BadRequest("Vui lòng nhập Mã sinh viên chủ hóa đơn để xác nhận.");
                    }
                }

                // 3. Gọi Service
                var generatedBienLai = await _thanhToanService.CreateThanhToanAsync(createDto);

                return CreatedAtAction("DownloadBienLai", "BienLai", new { maHd = createDto.MaHd }, generatedBienLai);
            }
            catch (Exception ex)
            {
                // Trả về BadRequest với thông báo lỗi chi tiết (ví dụ: "Hóa đơn không thuộc về SV này")
                return BadRequest(ex.Message);
            }
        }
    }
}
