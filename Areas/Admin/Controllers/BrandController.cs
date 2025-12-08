using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebsiteBanHang.Models;
using WebsiteBanHang.Repository;

namespace WebsiteBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class BrandController : Controller
    {
        private readonly IBrandRepository _brandRepository;

        public BrandController(IBrandRepository brandRepository)
        {
            _brandRepository = brandRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? search, int page = 1)
        {
            int pageSize = 10;
            var model = await _brandRepository.GetPagedBrandsAsync(search, page, pageSize);
            return View(model);
        }

        public async Task<IActionResult> Display(int id)
        {
            var brand = await _brandRepository.GetByIdAsync(id);
            if (brand == null)
                return NotFound();

            return View(brand);
        }

        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(Brand brand)
        {
            if (ModelState.IsValid)
            {
                await _brandRepository.AddAsync(brand);
                return RedirectToAction(nameof(Index));
            }
            return View(brand);
        }

        public async Task<IActionResult> Update(int id)
        {
            var brand = await _brandRepository.GetByIdAsync(id);
            if (brand == null)
                return NotFound();

            return View(brand);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int id, Brand brand)
        {
            if (id != brand.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                await _brandRepository.UpdateAsync(brand);
                return RedirectToAction(nameof(Index));
            }
            return View(brand);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var brand = await _brandRepository.GetByIdAsync(id);
            if (brand == null)
                return NotFound();

            return View(brand);
        }

        [HttpPost, ActionName("DeleteConfirmed")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _brandRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
