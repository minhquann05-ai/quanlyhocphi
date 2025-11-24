using QLHocPhi.DAL.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLHocPhi.DAL.Entities
{
    [Table("khoa")] 
    public class Khoa
    {
        [Key]
        [StringLength(10)]
        [Column("ma_khoa")] 
        public string? MaKhoa { get; set; } 

        [Required]
        [StringLength(100)]
        [Column("ten_khoa")]
        public string? TenKhoa { get; set; }

        public virtual ICollection<NganhHoc>? NganhHocs { get; set; }
    }
}
