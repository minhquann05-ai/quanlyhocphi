using Microsoft.AspNetCore.Mvc;
using QLHocPhi.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace QLHocPhi.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "PhongTaiChinh")]
    public class BaoCaoController : ControllerBase
    {
        private readonly IBaoCaoService _baoCaoService;

        public BaoCaoController(IBaoCaoService baoCaoService)
        {
            _baoCaoService = baoCaoService;
        }

        /// <summary>
        /// Xuất báo cáo công nợ theo học kỳ (PDF)
        /// </summary>
        // GET: api/BaoCao/CongNo?maHk=HK20251
        [HttpGet("CongNo")]
        public async Task<IActionResult> ExportCongNo([FromQuery] string maHk)
        {
            try
            {
                if (string.IsNullOrEmpty(maHk))
                    return BadRequest("Vui lòng cung cấp mã học kỳ (maHk).");

                // 1. Tạo file PDF
                byte[] pdfBytes = await _baoCaoService.ExportBaoCaoCongNoPdfAsync(maHk);

                // 2. Đặt tên file
                string fileName = $"BaoCaoCongNo_{maHk}_{DateTime.Now:yyyyMMdd}.pdf";

                // 3. Trả về file
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
