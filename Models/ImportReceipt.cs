using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CuaHangBanSach.Models
{
    public class ImportReceipt
    {
        public int Id { get; set; }

        public int SupplierId { get; set; }

        [ValidateNever]  // ✅ Bỏ qua validate navigation
        public Supplier Supplier { get; set; }

        public DateTime ImportDate { get; set; } = DateTime.Now;

        public string? Notes { get; set; }

        public decimal TotalAmount { get; set; }

        public string Status { get; set; } = "Đã đặt";

        // ✅ Bỏ qua validate navigation
        [ValidateNever]
        public List<ImportReceiptDetail> Details { get; set; } = new();
    }
}