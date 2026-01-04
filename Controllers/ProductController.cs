using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CuaHangBanSach.Models;
using CuaHangBanSach.Repository;
using CuaHangBanSach.ViewModels;

namespace CuaHangBanSach.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ApplicationDbContext _context;

        public ProductController(IProductRepository productRepository, ICategoryRepository categoryRepository, ApplicationDbContext context)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _context = context;
        }

        // KAN-FE-03: Tạo trang chi tiết sản phẩm
        public async Task<IActionResult> Index(string? search, string? sort, int? categoryId, int? brandId, int page = 1)
        {
            int pageSize = 6;

            var products = await _productRepository.GetAllAsync();

            // Lấy biến thể cho mỗi sản phẩm (nếu chưa có)
            foreach (var product in products)
            {
                product.Variants = _context.ProductVariants
                    .Where(v => v.ProductId == product.Id)
                    .ToList();
            }

            // Lọc theo danh mục
            if (categoryId.HasValue)
                products = products.Where(p => p.CategoryId == categoryId.Value).ToList();

            // Lọc theo thương hiệu
            if (brandId.HasValue)
                products = products.Where(p => p.BrandId == brandId.Value).ToList();

            // Tìm kiếm
            if (!string.IsNullOrEmpty(search))
            {
                products = products
                    .Where(p => p.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Sắp xếp
            products = sort switch
            {
                "price_asc" => products.OrderBy(p => p.Price).ToList(),
                "price_desc" => products.OrderByDescending(p => p.Price).ToList(),
                "oldest" => products.OrderBy(p => p.Id).ToList(),
                _ => products.OrderByDescending(p => p.Id).ToList() // mặc định: mới nhất
            };

            int totalRecords = products.Count();
            var pagedProducts = products
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var viewModel = new ProductListViewModel
            {
                Products = pagedProducts,
                SearchTerm = search,
                PageNumber = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                Sort = sort,
                CategoryId = categoryId,
                BrandId = brandId
            };

            // Load sliders for the sidebar
            ViewBag.Sliders = await _context.Sliders.ToListAsync();

            return View(viewModel);
        }

        // KAN-FE-03: Tạo trang chi tiết sản phẩm
        public async Task<IActionResult> Display(int id)
        {
            var product = await _productRepository.GetByIdWithVariantsAsync(id);
            if (product == null) return NotFound();

            var related = await _productRepository.GetRelatedProducts(product);

            var viewModel = new ProductDetailViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                ImageUrl = product.ImageUrl,
                Category = product.Category,
                Brand = product.Brand,
                Variants = product.Variants ?? new List<ProductVariant>(),
                Images = product.Images ?? new List<ProductImage>(),
                RelatedProducts = related ?? new List<Product>()
            };

            // Load sliders for the sidebar
            ViewBag.Sliders = await _context.Sliders.ToListAsync();

            return View(viewModel);
        }
    }
}