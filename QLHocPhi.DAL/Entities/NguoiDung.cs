using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLHocPhi.DAL.Entities
{
    [Table("nguoidung")]
    public class NguoiDung
    {
        [Key]
        [StringLength(50)]
        [Column("ten_dang_nhap")]
        public string? TenDangNhap { get; set; }

        [Required]
        [StringLength(100)]
        [Column("mat_khau")]
        public string? MatKhau { get; set; }

        [StringLength(30)]
        [Column("vai_tro")]
        public string? VaiTro { get; set; } // "SinhVien" hoặc "PhongTaiChinh"

        [StringLength(10)]
        [Column("ma_sv")]
        public string? MaSv { get; set; }

        [ForeignKey("MaSv")]
        public virtual SinhVien? SinhVien { get; set; }
    }
}
