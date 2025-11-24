using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLHocPhi.DAL.Entities
{
    [Table("bieuphi")]
    public class BieuPhi
    {
        [Key]
        [StringLength(10)]
        [Column("ma_bp")]
        public string? MaBp { get; set; }

        [StringLength(10)]
        [Column("ma_nganh")]
        public string? MaNganh { get; set; }

        [StringLength(10)]
        [Column("ma_hk")]
        public string? MaHk { get; set; }

        [Column("don_gia_tinchi", TypeName = "numeric(10, 2)")]
        public decimal DonGiaTinChi { get; set; }

        [ForeignKey("MaNganh")]
        public virtual NganhHoc? NganhHoc { get; set; }

        [ForeignKey("MaHk")]
        public virtual HocKy? HocKy { get; set; }
    }
}