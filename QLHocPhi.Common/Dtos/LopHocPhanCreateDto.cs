using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLHocPhi.Common.Dtos
{
    public class LopHocPhanCreateDto
    {
        [Required]
        [StringLength(15)]
        public string MaLhp { get; set; } // VD: 251_MH001_01

        [Required]
        [StringLength(100)]
        public string TenLhp { get; set; }

        [Required]
        public string MaMh { get; set; } // Phải thuộc môn học nào đó

        [Required]
        public string MaHk { get; set; } // Phải thuộc học kỳ nào đó

        [Range(1, 200)]
        public int SiSoToiDa { get; set; } = 60; // Mặc định 60
    }
}
