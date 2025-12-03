using QLHocPhi.DAL.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLHocPhi.DAL.Entities
{
    [Table("bienlai")]
    public class BienLai
    {
        [Key]
        [StringLength(10)]
        [Column("ma_bl")]
        public string? MaBl { get; set; }

        [StringLength(10)]
        [Column("ma_tt")]
        public string? MaTt { get; set; }

        [StringLength(20)]
        [Column("so_bien_lai")]
        public string? SoBienLai { get; set; } 

        [Column("ngay_in")]
        public DateTime? NgayIn { get; set; }

        [Column("noi_dung")]
        public string? NoiDung { get; set; }

        [ForeignKey("MaTt")]
        public virtual ThanhToan? ThanhToan { get; set; }
    }
}
