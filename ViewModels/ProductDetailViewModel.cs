using CuaHangBanSach.Models;
using System.Collections.Generic;

namespace CuaHangBanSach.ViewModels
{
    public class ProductDetailViewModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }

        public Category? Category { get; set; }
        public Brand? Brand { get; set; }

        public List<ProductVariant>? Variants { get; set; }
        public List<ProductImage>? Images { get; set; }

        public List<Product> RelatedProducts { get; set; } = new();
    }
}