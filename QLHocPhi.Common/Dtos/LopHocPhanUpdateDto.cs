using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLHocPhi.Common.Dtos
{
    public class LopHocPhanUpdateDto
    {
        [Required]
        [StringLength(100)]
        public string TenLhp { get; set; }

        [Range(1, 200)]
        public int SiSoToiDa { get; set; }
        public int? SiSoThucTe { get; set; }

        public string TrangThai { get; set; } // "Đang mở", "Đã khóa"...
    }
}
