using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;
using CuaHangBanSach.Models;
using CuaHangBanSach.Repository;
using CuaHangBanSach.ViewModels;

namespace CuaHangBanSach.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ImportReceiptController : Controller
    {
        private readonly IImportReceiptRepository _receiptRepository;
        private readonly ApplicationDbContext _context;

        public ImportReceiptController(IImportReceiptRepository receiptRepository, ApplicationDbContext context)
        {
            _receiptRepository = receiptRepository;
            _context = context;
        }

        public async Task<IActionResult> Index(string? search, int page = 1)
        {
            int pageSize = 10;
            var result = await _receiptRepository.GetPagedReceiptsAsync(search, page, pageSize);

            var model = new ImportReceiptListViewModel
            {
                Items = result.Items,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize,
                TotalRecords = result.TotalRecords,
                SearchTerm = search
            };

            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var receipt = await _receiptRepository.GetByIdAsync(id);
            if (receipt == null) return NotFound();
            return View(receipt);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var receipt = await _receiptRepository.GetByIdAsync(id);
            if (receipt == null) return NotFound();
            await _receiptRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var receipt = await _receiptRepository.GetByIdAsync(id);
            if (receipt == null) return NotFound();

            if (receipt.Status == "Đã hoàn thành" || receipt.Status == "Đã hủy")
            {
                TempData["Error"] = "Không thể cập nhật nếu đơn đã hoàn thành hoặc bị hủy.";
                return RedirectToAction("Index");
            }

            if (status == "Đã hoàn thành" && receipt.Status != "Đã hoàn thành")
            {
                foreach (var detail in receipt.Details)
                {
                    var variant = await _context.ProductVariants.FindAsync(detail.ProductVariantId);
                    if (variant != null)
                    {
                        variant.StockQuantity += detail.Quantity;
                    }
                }
                await _context.SaveChangesAsync();
            }

            receipt.Status = status;
            await _receiptRepository.UpdateAsync(receipt);

            TempData["Success"] = "Cập nhật trạng thái thành công.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Add()
        {
            ViewBag.Suppliers = await _context.Suppliers.OrderBy(s => s.Name).ToListAsync();
            ViewBag.Products = await _context.Products.Include(p => p.Variants).ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(ImportReceipt model)
        {
            Console.WriteLine("Model valid: " + ModelState.IsValid);
            Console.WriteLine("Details count: " + model.Details?.Count);

            if (!ModelState.IsValid || model.Details == null || model.Details.Count == 0)
            {
                ViewBag.Suppliers = await _context.Suppliers.OrderBy(s => s.Name).ToListAsync();
                ViewBag.Products = await _context.Products.Include(p => p.Variants).ToListAsync();
                TempData["Error"] = "Dữ liệu đơn nhập không hợp lệ.";
                return View(model);
            }

            model.TotalAmount = model.Details.Sum(d => d.TotalPrice);
            model.Status = "Đã đặt";

            await _receiptRepository.AddAsync(model);
            TempData["Success"] = "Tạo đơn nhập thành công.";
            return RedirectToAction("Index");
        }

        public IActionResult Print(int id)
        {
            var receipt = _context.ImportReceipts
                .Include(r => r.Supplier)
                .Include(r => r.Details)
                    .ThenInclude(d => d.ProductVariant)
                        .ThenInclude(v => v.Product)
                .FirstOrDefault(r => r.Id == id && r.Status == "Đã hoàn thành");

            if (receipt == null) return NotFound();

            return new ViewAsPdf("Print", receipt)
            {
                FileName = $"HoaDonNhap_{receipt.Id}.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4
            };
        }

    }
}