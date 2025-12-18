using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;
using CuaHangBanSach.Models;

public class OrdersDetailModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public OrdersDetailModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public Order Order { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        Order = await _context.Orders
            .Include(o => o.ApplicationUser)
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user.Id);

        if (Order == null) return NotFound();

        return Page();
    }

    public async Task<IActionResult> OnPostCancelAsync(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var order = await _context.Orders
            .Include(o => o.OrderDetails)
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user.Id);

        if (order == null)
        {
            TempData["Error"] = "Không tìm thấy đơn hàng.";
            return RedirectToPage(new { id });
        }

        if (order.Status == "Đang giao hàng" || order.Status == "Đã hoàn thành" || order.Status == "Đã hủy")
        {
            TempData["Error"] = "Đơn hàng đã được xử lý và không thể hủy.";
            return RedirectToPage(new { id });
        }

        order.Status = "Đã hủy";

        foreach (var item in order.OrderDetails)
        {
            var variant = await _context.ProductVariants
                .FirstOrDefaultAsync(v => v.ProductId == item.ProductId && v.Color == item.Color && v.Size == item.Size);

            if (variant != null)
            {
                variant.StockQuantity += item.Quantity;
                _context.ProductVariants.Update(variant);
            }
        }

        _context.Orders.Update(order);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Đơn hàng đã được hủy thành công.";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnGetPrintPdfAsync(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var order = await _context.Orders
            .Include(o => o.ApplicationUser)
            .Include(o => o.OrderDetails)
                .ThenInclude(d => d.Product)
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user.Id && o.Status == "Đã hoàn thành");

        if (order == null) return NotFound();

        // 👉 Dùng view truyền thống
        return new ViewAsPdf("Print", order)
        {
            FileName = $"HoaDon_{order.Id}.pdf",
            PageSize = Rotativa.AspNetCore.Options.Size.A4
        };
    }

}