using System.ComponentModel.DataAnnotations;

namespace CuaHangBanSach.Models
{
    public class Coupon
    {
        public int Id { get; set; }

        [Required]
        public string Code { get; set; }  // Ví dụ: "DISCOUNT50%"

        public decimal DiscountAmount { get; set; }  // Số tiền giảm (hoặc phần trăm nếu cần)

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool IsPercentage { get; set; }  // true = %, false = số tiền cố định

        public bool IsActive => DateTime.Now >= StartDate && DateTime.Now <= EndDate;
    }
}