using Microsoft.AspNetCore.Mvc;
using CuaHangBanSach.Models;
using CuaHangBanSach.Repository;

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

        public async Task<IActionResult> Index(string? search, string? sort, int? categoryId, int? brandId, int page = 1)
        {
            int pageSize = 6;

            var products = await _productRepository.GetAllAsync();

            // Lấy biến thể cho mỗi sản phẩm (nếu chưa có)
            // Note: ProductVariant và ProductImage sẽ được thêm vào sau trong các subtask tiếp theo

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

            // Note: ProductListViewModel sẽ được thêm vào sau trong các subtask tiếp theo
            // Tạm thời trả về danh sách sản phẩm trực tiếp
            ViewBag.SearchTerm = search;
            ViewBag.Sort = sort;
            ViewBag.CategoryId = categoryId;
            ViewBag.BrandId = brandId;
            ViewBag.PageNumber = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalRecords = totalRecords;

            return View(pagedProducts);
        }

        public async Task<IActionResult> Display(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return NotFound();

            // Note: ProductDetailViewModel sẽ được thêm vào sau trong các subtask tiếp theo
            // Tạm thời trả về sản phẩm trực tiếp
            return View(product);
        }
    }
}