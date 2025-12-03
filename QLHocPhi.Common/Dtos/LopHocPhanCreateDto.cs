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
        public string MaLhp { get; set; }   

        [Required]
        [StringLength(100)]
        public string TenLhp { get; set; }

        [Required]
        public string MaMh { get; set; }       

        [Required]
        public string MaHk { get; set; }       

        [Range(1, 200)]
        public int SiSoToiDa { get; set; } = 60;    
    }
}
