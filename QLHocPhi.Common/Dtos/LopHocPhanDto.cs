using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLHocPhi.Common.Dtos
{
    public class LopHocPhanDto
    {
        public string MaLhp { get; set; }
        public string TenLhp { get; set; }
        public string TenMonHoc { get; set; }
        public int SoTinChi { get; set; }
        public int SiSoToiDa { get; set; }
        public int SiSoThucTe { get; set; }
        public string MaMh { get; set; }
        public string MaHk { get; set; }
        public string MaNganh { get; set; }
        // Thêm thông tin để Frontend biết còn chỗ hay không
        public string TrangThaiSlot => SiSoThucTe >= SiSoToiDa ? "Hết chỗ" : "Còn chỗ";
    }
}
