using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CuaHangBanSach.Models;
using CuaHangBanSach.Repository;
using CuaHangBanSach.ViewModels;

namespace CuaHangBanSach.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class SupplierController : Controller
    {
        private readonly ISupplierRepository _supplierRepo;

        public SupplierController(ISupplierRepository supplierRepo)
        {
            _supplierRepo = supplierRepo;
        }

        // GET: Admin/Supplier
        public async Task<IActionResult> Index(string? search, int page = 1, int pageSize = 10)
        {
            var allSuppliers = await _supplierRepo.GetAllAsync(search);
            var pagedSuppliers = allSuppliers.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var viewModel = new SupplierListViewModel
            {
                Items = pagedSuppliers,
                PageNumber = page,
                PageSize = pageSize,
                TotalRecords = allSuppliers.Count,
                SearchTerm = search
            };

            return View(viewModel);
        }

        // GET: Admin/Supplier/Add
        public IActionResult Add()
        {
            return View();
        }

        // POST: Admin/Supplier/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(Supplier supplier)
        {
            if (!ModelState.IsValid)
                return View(supplier);

            await _supplierRepo.AddAsync(supplier);
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Supplier/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var supplier = await _supplierRepo.GetByIdAsync(id);
            if (supplier == null) return NotFound();
            return View(supplier);
        }

        // POST: Admin/Supplier/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Supplier supplier)
        {
            if (!ModelState.IsValid)
                return View(supplier);

            await _supplierRepo.UpdateAsync(supplier);
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Supplier/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var supplier = await _supplierRepo.GetByIdAsync(id);
            if (supplier == null) return NotFound();
            return View(supplier);
        }

        // POST: Admin/Supplier/Delete/5
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _supplierRepo.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Supplier/Display/5
        public async Task<IActionResult> Display(int id)
        {
            var supplier = await _supplierRepo.GetByIdAsync(id);
            if (supplier == null) return NotFound();
            return View(supplier);
        }
    }
}