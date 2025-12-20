// File: Areas/Admin/Controllers/SliderController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CuaHangBanSach.Models;
using CuaHangBanSach.ViewModels;

namespace CuaHangBanSach.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class SliderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SliderController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? search, int page = 1)
        {
            int pageSize = 10;
            var query = _context.Sliders.AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(s => s.Name.Contains(search));

            var totalRecords = await query.CountAsync();
            var items = await query
                .OrderByDescending(s => s.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var model = new SliderListViewModel
            {
                Items = items,
                PageNumber = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                SearchTerm = search
            };

            return View(model);
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(Slider slider)
        {
            if (ModelState.IsValid)
            {
                if (slider.ImageUpload != null && slider.ImageUpload.Length > 0)
                    slider.Image = await SaveImage(slider.ImageUpload);

                await _context.Sliders.AddAsync(slider);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(slider);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var slider = await _context.Sliders.FindAsync(id);
            if (slider == null) return NotFound();
            return View(slider);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Slider slider)
        {
            if (id != slider.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var existingSlider = await _context.Sliders.FindAsync(id);
                if (existingSlider == null) return NotFound();

                existingSlider.Name = slider.Name;
                existingSlider.Description = slider.Description;

                if (slider.ImageUpload != null && slider.ImageUpload.Length > 0)
                    existingSlider.Image = await SaveImage(slider.ImageUpload);

                _context.Sliders.Update(existingSlider);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(slider);
        }

        private async Task<string> SaveImage(IFormFile image)
        {
            var folderPath = Path.Combine("wwwroot", "sliders");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
            var filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
                await image.CopyToAsync(stream);

            return fileName;
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var slider = await _context.Sliders.FindAsync(id);
            if (slider == null) return NotFound();
            return View(slider);
        }

        [HttpPost, ActionName("DeleteConfirmed")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var slider = await _context.Sliders.FindAsync(id);
            if (slider == null) return NotFound();

            if (!string.IsNullOrEmpty(slider.Image))
            {
                string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "sliders", Path.GetFileName(slider.Image));
                if (System.IO.File.Exists(imagePath))
                    System.IO.File.Delete(imagePath);
            }

            _context.Sliders.Remove(slider);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}