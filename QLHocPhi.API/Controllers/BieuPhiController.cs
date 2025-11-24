using Microsoft.AspNetCore.Mvc;
using QLHocPhi.BLL.Interfaces;
using QLHocPhi.Common.Dtos;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BieuPhiController : ControllerBase
{
    private readonly IBieuPhiService _bieuPhiService;

    public BieuPhiController(IBieuPhiService bieuPhiService)
    {
        _bieuPhiService = bieuPhiService;
    }

    [HttpGet]
    [Authorize(Roles = "PhongTaiChinh,SinhVien")]
    public async Task<IActionResult> GetAllBieuPhi()
    {
        var result = await _bieuPhiService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("nganh/{maNganh}")]
    [Authorize(Roles = "PhongTaiChinh,SinhVien")]
    public async Task<IActionResult> GetBieuPhiByNganh(string maNganh)
    {
        try
        {
            var result = await _bieuPhiService.GetByNganhAsync(maNganh);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost]
    [Authorize(Roles = "PhongTaiChinh")]
    public async Task<IActionResult> CreateBieuPhi([FromBody] BieuPhiCreateDto createDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var newBieuPhi = await _bieuPhiService.CreateAsync(createDto);
            return Created($"api/BieuPhi/nganh/{newBieuPhi.MaNganh}", newBieuPhi);
        }

        catch (System.Exception ex)
        {
            var errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            return BadRequest(errorMessage);
        }
    }

    [HttpPut("{maBp}")]
    [Authorize(Roles = "PhongTaiChinh")]
    public async Task<IActionResult> UpdateBieuPhi(string maBp, [FromBody] BieuPhiUpdateDto updateDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            await _bieuPhiService.UpdateAsync(maBp, updateDto);
            return NoContent(); 
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (System.Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{maBp}")]
    [Authorize(Roles = "PhongTaiChinh")]
    public async Task<IActionResult> DeleteBieuPhi(string maBp)
    {
        try
        {
            await _bieuPhiService.DeleteAsync(maBp);
            return NoContent(); 
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (System.Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
