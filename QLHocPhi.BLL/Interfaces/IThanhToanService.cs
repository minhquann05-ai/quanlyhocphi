using QLHocPhi.Common.Dtos;

namespace QLHocPhi.BLL.Interfaces
{
    public interface IThanhToanService
    {
        Task<BienLaiDto> CreateThanhToanAsync(ThanhToanCreateDto createDto);
    }
}
