using CuaHangBanSach.Models;

namespace CuaHangBanSach.Repository
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product> GetByIdAsync(int id);
        Task<Product> GetByIdWithVariantsAsync(int id);

        Task AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(int id);

        Task AddProductImageAsync(ProductImage productImage);
        Task DeleteAllProductImagesAsync(int productId);

        Task<ProductListViewModel> GetPagedProductsAsync(string? search, int page, int pageSize);

        Task<List<Product>> GetRelatedProducts(Product product);
    }
}