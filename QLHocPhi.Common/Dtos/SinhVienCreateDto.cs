using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLHocPhi.Common.Dtos
{
    public class SinhVienCreateDto
    {
        [Required]
        [StringLength(10)]
        public string? MaSv { get; set; } 

        [Required]
        public string? HoTen { get; set; }

        public DateTime? NgaySinh { get; set; }
        public string? GioiTinh { get; set; }
        public string? MaLop { get; set; }
        public string? Email { get; set; }
        public string? Sdt { get; set; }
    }
}
