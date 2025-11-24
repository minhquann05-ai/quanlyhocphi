using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLHocPhi.DAL.Entities
{
    [Table("hocky")]
    public class HocKy
    {
        [Key]
        [StringLength(10)]
        [Column("ma_hk")]
        public string? MaHk { get; set; }

        [Required]
        [StringLength(20)]
        [Column("ten_hk")]
        public string? TenHk { get; set; }

        [Required]
        [StringLength(9)]
        [Column("nam_hoc")]
        public string? NamHoc { get; set; }

        [StringLength(20)]
        [Column("trang_thai")]
        public string? TrangThai { get; set; }

        public virtual ICollection<BieuPhi>? BieuPhis { get; set; }
    }
}
