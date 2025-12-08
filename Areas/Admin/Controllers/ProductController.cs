using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebsiteBanHang.Models;
using WebsiteBanHang.Repository;
using WebsiteBanHang.ViewModels;

namespace WebsiteBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly ApplicationDbContext _context;

        public ProductController(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IBrandRepository brandRepository,
            ApplicationDbContext context)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _brandRepository = brandRepository;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? search, int page = 1)
        {
            int pageSize = 7;
            var result = await _productRepository.GetPagedProductsAsync(search, page, pageSize);
            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> Add()
        {
            var categories = await _categoryRepository.GetAllAsync();
            var brands = await _brandRepository.GetAllAsync();

            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            ViewBag.Brands = new SelectList(brands, "Id", "Name");

            return View(new ProductWithVariantsViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(ProductWithVariantsViewModel model, IFormFile imageUrl)
        {
            // ✅ Nếu có ảnh chính, lưu trước rồi gán cho model.Product.ImageUrl
            if (imageUrl != null && imageUrl.Length > 0)
            {
                model.Product.ImageUrl = await SaveImage(imageUrl);
            }

            // ✅ Sau khi gán rồi mới kiểm tra ModelState
            if (ModelState.IsValid)
            {
                await _productRepository.AddAsync(model.Product);

                if (model.Variants != null && model.Variants.Any())
                {
                    foreach (var variantInput in model.Variants)
                    {
                        string? variantImageUrl = null;
                        if (variantInput.VariantImage != null)
                        {
                            variantImageUrl = await SaveImage(variantInput.VariantImage);
                        }

                        var variant = new ProductVariant
                        {
                            ProductId = model.Product.Id,
                            Color = variantInput.Color,
                            Size = variantInput.Size,
                            StockQuantity = variantInput.StockQuantity,
                            ImageUrl = variantImageUrl
                        };

                        _context.ProductVariants.Add(variant);
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            // Load lại dropdown nếu có lỗi
            ViewBag.Categories = new SelectList(await _categoryRepository.GetAllAsync(), "Id", "Name");
            ViewBag.Brands = new SelectList(await _brandRepository.GetAllAsync(), "Id", "Name");

            return View(model);
        }

        private async Task<string> SaveImage(IFormFile image)
        {
            var folderPath = Path.Combine("wwwroot", "images");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var fileName = Path.GetFileName(image.FileName);
            var filePath = Path.Combine(folderPath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }
            return "/images/" + fileName;
        }

        [HttpGet]
        public async Task<IActionResult> Display(int id)
        {
            var product = await _productRepository.GetByIdWithVariantsAsync(id);
            if (product == null)
                return NotFound();

            return View(product);
        }
        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            var product = await _productRepository.GetByIdWithVariantsAsync(id);
            if (product == null) return NotFound();

            var viewModel = new ProductWithVariantsViewModel
            {
                Product = product,
                Variants = product.Variants?.Select(v => new ProductVariantInputModel
                {
                    Id = v.Id,
                    Color = v.Color,
                    Size = v.Size,
                    StockQuantity = v.StockQuantity,
                    ImageUrl = v.ImageUrl
                }).ToList() ?? new()
            };

            ViewBag.Categories = new SelectList(await _categoryRepository.GetAllAsync(), "Id", "Name", product.CategoryId);
            ViewBag.Brands = new SelectList(await _brandRepository.GetAllAsync(), "Id", "Name", product.BrandId);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, ProductWithVariantsViewModel model, IFormFile? imageUrl)
        {
            if (id != model.Product.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var product = await _productRepository.GetByIdWithVariantsAsync(id);
                if (product == null) return NotFound();

                product.Name = model.Product.Name;
                product.Description = model.Product.Description;
                product.Price = model.Product.Price;
                product.CategoryId = model.Product.CategoryId;
                product.BrandId = model.Product.BrandId;

                if (imageUrl != null)
                    product.ImageUrl = await SaveImage(imageUrl);

                await _productRepository.UpdateAsync(product);

                // Xử lý biến thể
                var existingVariants = await _context.ProductVariants
                    .Where(v => v.ProductId == id)
                    .ToListAsync();

                _context.ProductVariants.RemoveRange(existingVariants);

                if (model.Variants != null && model.Variants.Any())
                {
                    foreach (var v in model.Variants)
                    {
                        string? imgUrl = null;
                        if (v.VariantImage != null)
                        {
                            imgUrl = await SaveImage(v.VariantImage);
                        }
                        else if (v.Id.HasValue)
                        {
                            var old = existingVariants.FirstOrDefault(x => x.Id == v.Id);
                            imgUrl = old?.ImageUrl;
                        }

                        _context.ProductVariants.Add(new ProductVariant
                        {
                            ProductId = product.Id,
                            Color = v.Color,
                            Size = v.Size,
                            StockQuantity = v.StockQuantity,
                            ImageUrl = imgUrl
                        });
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.Categories = new SelectList(await _categoryRepository.GetAllAsync(), "Id", "Name", model.Product.CategoryId);
            ViewBag.Brands = new SelectList(await _brandRepository.GetAllAsync(), "Id", "Name", model.Product.BrandId);
            return View(model);
        }


        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            return View(product);
        }

        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Xoá tất cả các biến thể của sản phẩm
            var variants = _context.ProductVariants.Where(v => v.ProductId == id);
            _context.ProductVariants.RemoveRange(variants);

            // Xoá toàn bộ ảnh phụ nếu có
            await _productRepository.DeleteAllProductImagesAsync(id);

            // Xoá chính sản phẩm
            await _productRepository.DeleteAsync(id);

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteVariant(int variantId, int productId)
        {
            var variant = await _context.ProductVariants.FindAsync(variantId);
            if (variant != null)
            {
                _context.ProductVariants.Remove(variant);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Update", new { id = productId });
        }
    }
}
