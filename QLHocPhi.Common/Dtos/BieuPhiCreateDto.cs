using System.ComponentModel.DataAnnotations;

namespace QLHocPhi.Common.Dtos
{
    public class BieuPhiCreateDto
    {
        [Required(ErrorMessage = "Mã ngành là bắt buộc")]
        public string? MaNganh { get; set; }

        [Required(ErrorMessage = "Mã học kỳ là bắt buộc")]
        public string? MaHk { get; set; }

        [Range(1, double.MaxValue, ErrorMessage = "Đơn giá phải lớn hơn 0")]
        public decimal DonGiaTinChi { get; set; }
    }
}
