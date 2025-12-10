using CuaHangBanSach.Models;

namespace CuaHangBanSach.ViewModels
{
    public class ProductListViewModel
    {
        public List<Product> Products { get; set; } = new();
        public string? SearchTerm { get; set; }
        public string? Sort { get; set; }

        public int? CategoryId { get; set; }
        public int? BrandId { get; set; }

        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
    }
}