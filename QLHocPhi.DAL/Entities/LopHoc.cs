using QLHocPhi.DAL.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLHocPhi.DAL.Entities
{
    [Table("lophoc")]
    public class LopHoc
    {
        [Key]
        [StringLength(15)]
        [Column("ma_lop")]
        public string? MaLop { get; set; }

        [Required]
        [StringLength(50)]
        [Column("ten_lop")]
        public string? TenLop { get; set; }

        [StringLength(10)]
        [Column("ma_nganh")]
        public string? MaNganh { get; set; }

        [StringLength(10)]
        [Column("khoa_hoc")]
        public string? KhoaHoc { get; set; }

        [ForeignKey("MaNganh")]
        public virtual NganhHoc? NganhHoc { get; set; }

        public virtual ICollection<SinhVien>? SinhViens { get; set; }
    }
}
