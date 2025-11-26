using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLHocPhi.Common.Dtos
{
    public class SinhVienSearchDto
    {
        public string? MaSv { get; set; }
        public string? HoTen { get; set; }
        public string? MaLop { get; set; }
        public string? Email { get; set; }
        public string? Sdt { get; set; }
        public string? GioiTinh { get; set; }
        public DateTime? NgaySinh { get; set; }
    }
}
