using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;

namespace CuaHangBanSach.Models
{
    public class Order
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public DateTime OrderDate { get; set; }

        public decimal TotalPrice { get; set; }

        public string? CouponCode { get; set; } // Mã giảm giá được sử dụng (nếu có)

        public decimal DiscountAmount { get; set; } = 0; // Số tiền được giảm


        public string ShippingAddress { get; set; }

        public string? CustomerName { get; set; } // <-- mới
        public string? PhoneNumber { get; set; }  // <-- mới

        public string? Notes { get; set; }

        public string Status { get; set; } = "Chờ xác nhận";

        [ForeignKey("UserId")]
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }

        public List<OrderDetail> OrderDetails { get; set; }
    }
}