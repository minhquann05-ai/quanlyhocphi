using QLHocPhi.DAL.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLHocPhi.DAL.Entities
{
    [Table("chitiethoadon")]
    public class ChiTietHoaDon
    {
        [Key]
        [StringLength(10)]
        [Column("ma_ct")]
        public string? MaCt { get; set; }

        [StringLength(10)]
        [Column("ma_hd")]
        public string? MaHd { get; set; }

        [StringLength(200)]
        [Column("noi_dung")]
        public string? NoiDung { get; set; }

        [Column("so_tien", TypeName = "numeric(12, 2)")]
        public decimal? SoTien { get; set; }

        [ForeignKey("MaHd")]
        public virtual HoaDon? HoaDon { get; set; }
    }
}
