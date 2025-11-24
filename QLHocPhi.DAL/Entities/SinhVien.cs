using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLHocPhi.DAL.Entities
{
    [Table("sinhvien")]
    public class SinhVien
    {
        [Key]
        [StringLength(10)]
        [Column("ma_sv")]
        public string? MaSv { get; set; }

        [Required]
        [StringLength(100)]
        [Column("ho_ten")]
        public string? HoTen { get; set; }

        [Column("ngay_sinh", TypeName = "date")]
        public DateTime? NgaySinh { get; set; }

        [StringLength(30)]
        [Column("gioi_tinh")]
        public string? GioiTinh { get; set; }

        [StringLength(15)]
        [Column("ma_lop")]
        public string? MaLop { get; set; }
        [StringLength(100)]
        [Column("email")]
        public string? Email { get; set; }

        [StringLength(15)]
        [Column("sdt")]
        public string? Sdt { get; set; }

        [ForeignKey("MaLop")]
        public virtual LopHoc? LopHoc { get; set; }

        public virtual ICollection<HoaDon>? HoaDons { get; set; }

    }
}
