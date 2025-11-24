using QLHocPhi.Common.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QLHocPhi.BLL.Interfaces
{
    public interface IBieuPhiService
    {
        Task<IEnumerable<BieuPhiDto>> GetAllAsync();
        Task<IEnumerable<BieuPhiDto>> GetByNganhAsync(string maNganh);
        Task<BieuPhiDto> CreateAsync(BieuPhiCreateDto createDto);
        Task UpdateAsync(string maBp, BieuPhiUpdateDto updateDto);
        Task DeleteAsync(string maBp);
    }
}
