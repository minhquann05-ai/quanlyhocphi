using Microsoft.AspNetCore.Mvc;
using QLHocPhi.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace QuanLyHocPhi.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BienLaiController : ControllerBase
    {
        private readonly IBienLaiService _bienLaiService;

        public BienLaiController(IBienLaiService bienLaiService)
        {
            _bienLaiService = bienLaiService;
        }

        [HttpPost("sync-missing")]
        [Authorize(Roles = "PhongTaiChinh")] // Chỉ Admin được chạy
        public async Task<IActionResult> SyncMissingBienLai()
        {
            try
            {
                int count = await _bienLaiService.SyncMissingBienLaiAsync();
                return Ok(new { message = $"Đã đồng bộ thành công. Tạo mới {count} biên lai." });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Lấy tất cả biên lai (Chỉ dành cho Phòng Tài Chính)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "PhongTaiChinh")] // Khóa chặt chức năng này
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _bienLaiService.GetAllAsync());
        }

        /// <summary>
        /// Lấy danh sách biên lai của một sinh viên (Cả 2 đều dùng được)
        /// </summary>
        // GET: api/BienLai/sinhvien/2351010041
        [HttpGet("sinhvien")] // Bỏ {maSv} để tham số trở thành Query String (tùy chọn)
        [Authorize(Roles = "PhongTaiChinh,SinhVien")]
        public async Task<IActionResult> GetByMaSv([FromQuery] string? maSv)
        {
            // 1. Lấy thông tin từ Token
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var tokenMaSv = User.FindFirst("MaSv")?.Value;

            string targetMaSv = maSv;

            // 2. Xử lý logic phân quyền
            if (role == "SinhVien")
            {
                // Nếu là Sinh viên: BẮT BUỘC dùng mã SV trong Token (để bảo mật)
                targetMaSv = tokenMaSv;
            }
            else if (role == "PhongTaiChinh")
            {
                // Nếu là Phòng Tài chính: Bắt buộc phải nhập maSv muốn xem
                if (string.IsNullOrEmpty(targetMaSv))
                {
                    return BadRequest("Vui lòng nhập mã sinh viên cần tra cứu.");
                }
            }

            // 3. Kiểm tra cuối cùng
            if (string.IsNullOrEmpty(targetMaSv))
            {
                return BadRequest("Không xác định được mã sinh viên.");
            }

            // 4. Gọi Service
            var result = await _bienLaiService.GetByMaSvAsync(targetMaSv);
            return Ok(result);
        }

        /// <summary>
        /// Tải file PDF biên lai (Cả 2 đều dùng được)
        /// </summary>
        // GET: api/BienLai/download/HD0001
        [HttpGet("download/{maHd}")]
        // Cho phép cả 2 role truy cập
        [Authorize(Roles = "PhongTaiChinh,SinhVien")]
        public async Task<IActionResult> DownloadBienLai(string maHd)
        {
            try
            {
                byte[] pdfBytes = await _bienLaiService.GenerateBienLaiPdfAsync(maHd);
                string fileName = $"BienLai_{maHd}_{DateTime.UtcNow:yyyyMMdd}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
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
