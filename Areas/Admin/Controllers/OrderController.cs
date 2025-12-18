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
    public class OrderController : Controller
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ApplicationDbContext _context;

        public OrderController(IOrderRepository orderRepository, ApplicationDbContext context)
        {
            _context = context;
            _orderRepository = orderRepository;
        }

        // Hiển thị danh sách đơn hàng với tìm kiếm + phân trang
        public async Task<IActionResult> Index(string? search, int page = 1)
        {
            int pageSize = 10;
            var (orders, totalRecords) = await _orderRepository.GetPagedOrdersAsync(page, pageSize, search);

            var model = new OrderListViewModel
            {
                Items = orders,
                PageNumber = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                SearchTerm = search
            };

            return View(model);
        }

        // Hiển thị chi tiết đơn hàng
        public async Task<IActionResult> Details(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
                return NotFound();

            return View(order);
        }

        // Xóa đơn hàng
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
                return NotFound();

            await _orderRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // Cập nhật trạng thái đơn hàng
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int orderId, string status)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
                return NotFound();

            if (order.Status == "Đã hoàn thành" || order.Status == "Đã hủy")
            {
                TempData["Error"] = "Không thể thay đổi trạng thái khi đơn hàng đã hoàn thành hoặc đã bị hủy.";
                return RedirectToAction("Index");
            }

            if (status == "Đã hủy" && order.Status != "Đã hủy")
            {
                foreach (var item in order.OrderDetails)
                {
                    var variant = await _context.ProductVariants
                        .FirstOrDefaultAsync(v => v.ProductId == item.ProductId && v.Color == item.Color && v.Size == item.Size);

                    if (variant != null)
                    {
                        variant.StockQuantity += item.Quantity;
                    }
                }
                await _context.SaveChangesAsync();
            }

            order.Status = status;
            await _orderRepository.UpdateAsync(order);

            TempData["Success"] = "Cập nhật trạng thái thành công.";
            return RedirectToAction("Index");
        }

        public IActionResult Print(int id)
        {
            var order = _context.Orders
                .Include(o => o.ApplicationUser)
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.Product)
                .FirstOrDefault(o => o.Id == id && o.Status == "Đã hoàn thành");

            if (order == null) return NotFound();

            return new ViewAsPdf("Print", order)
            {
                FileName = $"HoaDonBan_{order.Id}.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4
            };
        }
    }
}