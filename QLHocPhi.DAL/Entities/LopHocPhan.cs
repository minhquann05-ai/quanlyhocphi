using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLHocPhi.DAL.Entities
{
    [Table("lop_hoc_phan")]
    public class LopHocPhan
    {
        [Key]
        [StringLength(15)]
        [Column("ma_lhp")]
        public string? MaLhp { get; set; }

        [Required]
        [StringLength(100)]
        [Column("ten_lhp")]
        public string? TenLhp { get; set; }

        [StringLength(10)]
        [Column("ma_mh")]
        public string? MaMh { get; set; }

        [StringLength(10)]
        [Column("ma_hk")]
        public string? MaHk { get; set; }

        [Column("si_so_toi_da")]
        public int SiSoToiDa { get; set; }

        [Column("si_so_thuc_te")]
        public int SiSoThucTe { get; set; }

        [StringLength(20)]
        [Column("trang_thai")]
        public string? TrangThai { get; set; }

        // Navigation
        [ForeignKey("MaMh")]
        public virtual MonHoc? MonHoc { get; set; }

        [ForeignKey("MaHk")]
        public virtual HocKy? HocKy { get; set; }
        public virtual ICollection<DangKyHocPhan> DangKyHocPhans { get; set; }
    }
}
