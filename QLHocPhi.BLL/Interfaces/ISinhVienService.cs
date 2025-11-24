using QLHocPhi.Common.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLHocPhi.BLL.Interfaces
{
    public interface ISinhVienService
    {
        Task<IEnumerable<SinhVienDto>> GetAllAsync();
        Task<SinhVienDto> GetByIdAsync(string maSv);
        Task<SinhVienDto> CreateAsync(SinhVienCreateDto createDto);
        Task UpdateAsync(string maSv, SinhVienUpdateDto updateDto);
        Task DeleteAsync(string maSv);
    }
}
