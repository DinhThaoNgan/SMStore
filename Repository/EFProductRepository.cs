using Microsoft.EntityFrameworkCore;
using CuaHangBanSach.Models;
using CuaHangBanSach.ViewModels;

namespace CuaHangBanSach.Repository
{
    public class EFProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public EFProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Variants)
                .ToListAsync();
        }

        public async Task<Product> GetByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Product> GetByIdWithVariantsAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task AddAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddProductImageAsync(ProductImage productImage)
        {
            _context.ProductImages.Add(productImage);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAllProductImagesAsync(int productId)
        {
            var images = _context.ProductImages.Where(pi => pi.ProductId == productId);
            _context.ProductImages.RemoveRange(images);
            await _context.SaveChangesAsync();
        }

        public async Task<ProductListViewModel> GetPagedProductsAsync(string? search, int page, int pageSize)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.Contains(search));
            }

            var totalRecords = await query.CountAsync();

            var products = await query
                .OrderBy(p => p.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            foreach (var product in products)
            {
                product.Variants = await _context.ProductVariants
                    .Where(v => v.ProductId == product.Id)
                    .ToListAsync();
            }

            return new ProductListViewModel
            {
                Products = products,
                SearchTerm = search,
                PageNumber = page,
                PageSize = pageSize,
                TotalRecords = totalRecords
            };
        }

        public async Task<List<Product>> GetRelatedProducts(Product product)
        {
            return await _context.Products
                .Where(p => p.Id != product.Id &&
                            (p.CategoryId == product.CategoryId || p.BrandId == product.BrandId))
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Take(10)
                .ToListAsync();
        }
    }
}