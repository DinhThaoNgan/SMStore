using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Rotativa.AspNetCore;
using CuaHangBanSach.Models;

public class OrdersController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public OrdersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Print(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var order = await _context.Orders
            .Include(o => o.ApplicationUser)
            .Include(o => o.OrderDetails)
                .ThenInclude(d => d.Product)
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user.Id && o.Status == "Đã hoàn thành");

        if (order == null) return NotFound();

        return new ViewAsPdf("Print", order) // không phải đường dẫn, chỉ tên View!
        {
            FileName = $"HoaDon_{order.Id}.pdf",
            PageSize = Rotativa.AspNetCore.Options.Size.A4
        };
    }
}
