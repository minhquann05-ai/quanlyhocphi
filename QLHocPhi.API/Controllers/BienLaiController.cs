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
        [Authorize(Roles = "PhongTaiChinh")]     
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

        [HttpGet]
        [Authorize(Roles = "PhongTaiChinh")]      
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _bienLaiService.GetAllAsync());
        }

        [HttpGet("sinhvien")]            
        [Authorize(Roles = "PhongTaiChinh,SinhVien")]
        public async Task<IActionResult> GetByMaSv([FromQuery] string? maSv)
        {
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
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
                    return BadRequest("Vui lòng nhập mã sinh viên cần tra cứu.");
                }
            }

            if (string.IsNullOrEmpty(targetMaSv))
            {
                return BadRequest("Không xác định được mã sinh viên.");
            }

            var result = await _bienLaiService.GetByMaSvAsync(targetMaSv);
            return Ok(result);
        }

        [HttpGet("download/{maHd}")]
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
