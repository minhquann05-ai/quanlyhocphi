using QLHocPhi.DAL.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLHocPhi.DAL.Entities
{
    [Table("thanhtoan")]
    public class ThanhToan
    {
        [Key]
        [StringLength(10)]
        [Column("ma_tt")]
        public string? MaTt { get; set; }

        [StringLength(10)]
        [Column("ma_hd")]
        public string? MaHd { get; set; }

        [Column("ngay_tt")]
        public DateTime? NgayTt { get; set; }

        [Column("so_tien_tt", TypeName = "numeric(12, 2)")]
        public decimal? SoTienTt { get; set; }

        [StringLength(50)]
        [Column("phuong_thuc")]
        public string? PhuongThuc { get; set; }

        [StringLength(30)]
        [Column("trang_thai_tt")]
        public string? TrangThaiTt { get; set; }

        [ForeignKey("MaHd")]
        public virtual HoaDon? HoaDon { get; set; }

        public virtual ICollection<BienLai>? BienLais { get; set; }
    }
}
