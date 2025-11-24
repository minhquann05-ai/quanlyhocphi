using System.ComponentModel.DataAnnotations;

namespace QLHocPhi.Common.Dtos
{
    public class BieuPhiUpdateDto
    {
        [Range(1, double.MaxValue, ErrorMessage = "Đơn giá phải lớn hơn 0")]
        public decimal DonGiaTinChi { get; set; }
    }
}
