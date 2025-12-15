using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CuaHangBanSach.Models
{
    public class ImportReceiptDetail
    {
        public int Id { get; set; }

        public int ImportReceiptId { get; set; }

        [ValidateNever]  // ✅ Bỏ qua validate navigation
        public ImportReceipt ImportReceipt { get; set; }

        public int ProductVariantId { get; set; }

        [ValidateNever]  // ✅ Bỏ qua validate navigation
        public ProductVariant ProductVariant { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal TotalPrice => Quantity * UnitPrice;
    }
}