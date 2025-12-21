using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using CuaHangBanSach.Models;

namespace CuaHangBanSach.ViewModels
{
    public class ProductVariantInputModel
    {
        public int? Id { get; set; }

        [Required]
        public string Color { get; set; }

        [Required]
        public string Size { get; set; }

        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        public IFormFile? VariantImage { get; set; }
        public string? ImageUrl { get; set; }

        public string? ExistingImageUrl { get; set; } // để giữ ảnh cũ nếu không cập nhật
    }

    public class ProductWithVariantsViewModel
    {
        public Product Product { get; set; } = new Product();

        public List<ProductVariantInputModel> Variants { get; set; } = new();
    }
}