using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLHocPhi.Common.Dtos
{
    public class KetQuaDangKyDto
    {
        public string MaLhp { get; set; }
        public string TenLhp { get; set; }
        public string TenMonHoc { get; set; }
        public int SoTinChi { get; set; }
        public DateTime NgayDangKy { get; set; }
        public decimal HocPhi { get; set; }
    }
}
