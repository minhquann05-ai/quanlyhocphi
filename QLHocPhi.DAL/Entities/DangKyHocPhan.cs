using QLHocPhi.DAL.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLHocPhi.DAL.Entities
{
    [Table("dangkyhocphan")]
    public class DangKyHocPhan
    {
        [Key]
        [StringLength(10)]
        [Column("ma_dk")]
        public string MaDk { get; set; }

        [StringLength(10)]
        [Column("ma_sv")]
        public string MaSv { get; set; }

        [StringLength(15)]
        [Column("ma_lhp")]
        public string MaLhp { get; set; }

        [StringLength(10)]
        [Column("ma_hk")]
        public string? MaHk { get; set; }

        [Column("ngay_dk")]
        public DateTime? NgayDk { get; set; }

        [StringLength(20)]
        [Column("trang_thai")]
        public string? TrangThai { get; set; }

        [ForeignKey("MaSv")]
        public virtual SinhVien? SinhVien { get; set; }


        [ForeignKey("MaHk")]
        public virtual HocKy? HocKy { get; set; }
        [ForeignKey("MaLhp")]
        public virtual LopHocPhan LopHocPhan { get; set; }
    }
}
