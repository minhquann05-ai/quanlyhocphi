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
    }
}