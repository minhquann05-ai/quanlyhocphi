using QLHocPhi.Common.Dtos;

namespace QLHocPhi.BLL.Interfaces
{
    public interface IDangKyHocPhanService
    {
        Task<HoaDonDto> CreateDangKyAsync(DangKyHocPhanCreateDto createDto);
    }
}
