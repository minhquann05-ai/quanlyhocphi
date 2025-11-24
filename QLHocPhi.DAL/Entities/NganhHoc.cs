using QLHocPhi.DAL.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLHocPhi.DAL.Entities
{
    [Table("nganhhoc")]
    public class NganhHoc
    {
        [Key]
        [StringLength(10)]
        [Column("ma_nganh")]
        public string? MaNganh { get; set; }

        [Required]
        [StringLength(100)]
        [Column("ten_nganh")]
        public string? TenNganh { get; set; }

        [StringLength(10)]
        [Column("ma_khoa")]
        public string? MaKhoa { get; set; }

        [ForeignKey("MaKhoa")]
        public virtual Khoa? Khoa { get; set; }

        public virtual ICollection<BieuPhi>? BieuPhis { get; set; }
    }
}
