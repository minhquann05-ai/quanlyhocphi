using QLHocPhi.DAL.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLHocPhi.DAL.Entities
{
    [Table("monhoc")]
    public class MonHoc
    {
        [Key]
        [StringLength(10)]
        [Column("ma_mh")]
        public string? MaMh { get; set; }

        [Required]
        [StringLength(100)]
        [Column("ten_mh")]
        public string? TenMh { get; set; }

        [Column("so_tin_chi")]
        public int? SoTinChi { get; set; }

        [StringLength(10)]
        [Column("ma_nganh")]
        public string? MaNganh { get; set; }

        [ForeignKey("MaNganh")]
        public virtual NganhHoc? NganhHoc { get; set; }
        public virtual ICollection<LopHocPhan> LopHocPhans { get; set; }
    }
}
