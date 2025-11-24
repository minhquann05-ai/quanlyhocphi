using QLHocPhi.Common.Dtos;

namespace QLHocPhi.BLL.Interfaces
{
    public interface IHoaDonService
    {
        Task<IEnumerable<HoaDonDto>> GetAllAsync(string? trangThai = null);

        Task<IEnumerable<HoaDonDto>> GetByMaSvAsync(string maSv, string trangThai);

        Task<HoaDonDto> CreateAsync(HoaDonCreateDto createDto);
        Task UpdateTrangThaiAsync(string maHd, HoaDonUpdateDto updateDto);
        Task DeleteAsync(string maHd);
    }
}
