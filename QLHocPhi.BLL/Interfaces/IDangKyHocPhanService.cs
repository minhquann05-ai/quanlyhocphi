using QLHocPhi.Common.Dtos;

namespace QLHocPhi.BLL.Interfaces
{
    public interface IDangKyHocPhanService
    {
        Task<HoaDonDto> CreateDangKyAsync(DangKyHocPhanCreateDto createDto);
        Task<IEnumerable<LopHocPhanDto>> GetAvailableClassesForStudentAsync(string maSv);
        Task CancelRegistrationAsync(string maSv, string maLhp, string role = null);
        Task<IEnumerable<KetQuaDangKyDto>> GetRegisteredClassesAsync(string maSv);
    }
}
