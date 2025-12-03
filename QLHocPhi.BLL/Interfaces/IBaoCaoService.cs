using QLHocPhi.Common.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLHocPhi.BLL.Interfaces
{
    public interface IBaoCaoService
    {
        Task<byte[]> ExportBaoCaoCongNoPdfAsync(string maHk);

        Task<List<BaoCaoCongNoDto>> GetListBaoCaoAsync(string maHk, string? maSv);
    }
}
