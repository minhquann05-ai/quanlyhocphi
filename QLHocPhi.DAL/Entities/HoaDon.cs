using QLHocPhi.DAL.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLHocPhi.DAL.Entities
{
    [Table("hoadon")]
    public class HoaDon
    {
        [Key]
        [StringLength(10)]
        [Column("ma_hd")]
        public string? MaHd { get; set; }

        [StringLength(10)]
        [Column("ma_sv")]
        public string? MaSv { get; set; }

        [StringLength(10)]
        [Column("ma_hk")]
        public string? MaHk { get; set; }

        [Column("ngay_tao")]
        public DateTime? NgayTao { get; set; }

        [Column("tong_tien", TypeName = "numeric(12, 2)")]
        public decimal? TongTien { get; set; }

        [StringLength(30)]
        [Column("trang_thai")]
        public string? TrangThai { get; set; }

        [ForeignKey("MaSv")]
        public virtual SinhVien? SinhVien { get; set; }

        [ForeignKey("MaHk")]
        public virtual HocKy? HocKy { get; set; }

        public virtual ICollection<ChiTietHoaDon>? ChiTietHoaDons { get; set; }
    }
}
