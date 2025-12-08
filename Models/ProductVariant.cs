using System.ComponentModel.DataAnnotations;

namespace CuaHangBanSach.Models
{
    public class ProductVariant
    {
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }
        public Product Product { get; set; }

        [Required, StringLength(100)]
        public string Color { get; set; }
        public string? ImageUrl { get; set; }

        [Required, StringLength(100)]
        public string Size { get; set; }

        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }
    }
}