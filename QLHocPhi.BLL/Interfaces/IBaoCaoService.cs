using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLHocPhi.BLL.Interfaces
{
    public interface IBaoCaoService
    {
        // Nhận vào Mã Học Kỳ, trả về file PDF
        Task<byte[]> ExportBaoCaoCongNoPdfAsync(string maHk);
    }
}
