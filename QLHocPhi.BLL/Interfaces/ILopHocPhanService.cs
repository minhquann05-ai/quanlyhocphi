using QLHocPhi.Common.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLHocPhi.BLL.Interfaces
{
    public interface ILopHocPhanService
    {
        Task<IEnumerable<LopHocPhanDto>> GetAllAsync();
        Task<IEnumerable<LopHocPhanDto>> GetByNganhAsync(string maNganh);
        Task<LopHocPhanDto> CreateAsync(LopHocPhanCreateDto createDto);
        Task UpdateAsync(string maLhp, LopHocPhanUpdateDto updateDto);
        Task DeleteAsync(string maLhp);
    }
}
