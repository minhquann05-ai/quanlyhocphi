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

        [HttpGet("CongNo")]
        public async Task<IActionResult> ExportCongNo([FromQuery] string maHk)
        {
            try
            {
                if (string.IsNullOrEmpty(maHk))
                    return BadRequest("Vui lòng cung cấp mã học kỳ (maHk).");

                byte[] pdfBytes = await _baoCaoService.ExportBaoCaoCongNoPdfAsync(maHk);

                string fileName = $"BaoCaoCongNo_{maHk}_{DateTime.Now:yyyyMMdd}.pdf";

                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetCongNoData")]
        public async Task<IActionResult> GetCongNoData([FromQuery] string maHk, [FromQuery] string? maSv)
        {
            try
            {
                var data = await _baoCaoService.GetListBaoCaoAsync(maHk, maSv);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
