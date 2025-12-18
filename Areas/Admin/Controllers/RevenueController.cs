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
    public class RevenueController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IImportReceiptRepository _importRepo;

        public RevenueController(ApplicationDbContext context, IImportReceiptRepository importRepo)
        {
            _context = context;
            _importRepo = importRepo;
        }

        public IActionResult Index()
        {
            var model = new RevenueFilterViewModel
            {
                FilterType = "day",
                FromDate = null,
                ToDate = null,
                Labels = new(),
                Revenues = new(),
                ImportCosts = new()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Filter(RevenueFilterViewModel model)
        {
            model.Labels = new();
            model.Revenues = new();
            model.ImportCosts = new();

            var orders = _context.Orders
                .Include(o => o.OrderDetails)
                .Where(o => o.Status == "Đã hoàn thành");

            if (model.FilterType == "day" && model.FromDate.HasValue && model.ToDate.HasValue)
            {
                var from = model.FromDate.Value.Date;
                var to = model.ToDate.Value.Date;

                orders = orders.Where(o => o.OrderDate >= from && o.OrderDate <= to);

                var orderGrouped = await orders
                    .GroupBy(o => o.OrderDate.Date)
                    .Select(g => new { Date = g.Key, Revenue = g.Sum(x => x.TotalPrice) })
                    .ToListAsync();

                var importGrouped = await _importRepo.GetImportCostsByDay(from, to);
                var allDates = Enumerable.Range(0, (to - from).Days + 1)
                    .Select(offset => from.AddDays(offset)).ToList();

                foreach (var date in allDates)
                {
                    model.Labels.Add(date.ToString("dd/MM/yyyy"));
                    var revenue = orderGrouped.FirstOrDefault(d => d.Date == date)?.Revenue ?? 0;

                    var importItem = importGrouped.FirstOrDefault(d => d.Date == date);
                    var importCost = importItem != default ? importItem.TotalCost : 0;

                    model.Revenues.Add(revenue);
                    model.ImportCosts.Add(importCost);
                }
            }
            else if (model.FilterType == "month" &&
                     model.FromMonth.HasValue && model.FromMonthYear.HasValue &&
                     model.ToMonth.HasValue && model.ToMonthYear.HasValue)
            {
                var from = new DateTime(model.FromMonthYear.Value, model.FromMonth.Value, 1);
                var to = new DateTime(model.ToMonthYear.Value, model.ToMonth.Value, 1);
                orders = orders.Where(o => o.OrderDate >= from && o.OrderDate < to.AddMonths(1));

                var orderGrouped = await orders
                    .GroupBy(o => new { o.OrderDate.Month, o.OrderDate.Year })
                    .Select(g => new { g.Key.Month, g.Key.Year, Revenue = g.Sum(x => x.TotalPrice) })
                    .ToListAsync();

                var importGrouped = await _importRepo.GetImportCostsByMonth(from, to);
                var monthCount = ((to.Year - from.Year) * 12 + to.Month - from.Month + 1);

                for (int i = 0; i < monthCount; i++)
                {
                    var monthDate = from.AddMonths(i);
                    var label = $"{monthDate.Month:00}/{monthDate.Year}";
                    model.Labels.Add(label);

                    var revenue = orderGrouped
                        .FirstOrDefault(m => m.Month == monthDate.Month && m.Year == monthDate.Year)?.Revenue ?? 0;

                    var importItem = importGrouped
                        .FirstOrDefault(m => m.Month == monthDate.Month && m.Year == monthDate.Year);
                    var importCost = importItem != default ? importItem.TotalCost : 0;

                    model.Revenues.Add(revenue);
                    model.ImportCosts.Add(importCost);
                }
            }
            else if (model.FilterType == "year" &&
                     model.FromYear.HasValue && model.ToYear.HasValue)
            {
                if (model.ToYear < model.FromYear || (model.ToYear - model.FromYear + 1) > 10)
                {
                    ModelState.AddModelError("", "Khoảng năm không hợp lệ (tối đa 10 năm)");
                    return View("Index", model);
                }

                orders = orders.Where(o => o.OrderDate.Year >= model.FromYear && o.OrderDate.Year <= model.ToYear);

                var orderGrouped = await orders
                    .GroupBy(o => o.OrderDate.Year)
                    .Select(g => new { Year = g.Key, Revenue = g.Sum(x => x.TotalPrice) })
                    .ToListAsync();

                var importGrouped = await _importRepo.GetImportCostsByYear(model.FromYear.Value, model.ToYear.Value);

                for (int year = model.FromYear.Value; year <= model.ToYear.Value; year++)
                {
                    model.Labels.Add(year.ToString());

                    var revenue = orderGrouped.FirstOrDefault(y => y.Year == year)?.Revenue ?? 0;

                    var importItem = importGrouped.FirstOrDefault(y => y.Year == year);
                    var importCost = importItem != default ? importItem.TotalCost : 0;

                    model.Revenues.Add(revenue);
                    model.ImportCosts.Add(importCost);
                }
            }

            var list = await orders.ToListAsync();
            model.TotalOrders = list.Count;
            model.TotalProducts = list.SelectMany(o => o.OrderDetails).Sum(d => d.Quantity);
            model.TotalRevenue = list.Sum(o => o.TotalPrice);
            model.TotalImportCost = model.ImportCosts.Sum();

            return View("Index", model);
        }

        [HttpPost]
        public async Task<IActionResult> ExportPdf(RevenueFilterViewModel model, string ChartImageBase64)
        {
            var result = await Filter(model) as ViewResult;
            var filteredModel = result?.Model as RevenueFilterViewModel;
            ViewData["ChartImageBase64"] = ChartImageBase64;

            return new ViewAsPdf("ExportPdf", filteredModel)
            {
                FileName = "ThongKeDoanhThu.pdf",
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape
            };
        }
    }
}