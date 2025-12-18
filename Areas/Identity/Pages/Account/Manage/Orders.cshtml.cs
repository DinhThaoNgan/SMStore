using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;
using CuaHangBanSach.Areas.Identity.Pages.Account.Manage;
using CuaHangBanSach.Models;
using CuaHangBanSach.Repository;
using CuaHangBanSach.ViewModels;

public class OrdersModel : PageModel
{
    private readonly IOrderRepository _orderRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public OrdersModel(IOrderRepository orderRepository, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
    {
        _orderRepository = orderRepository;
        _userManager = userManager;
        _context = context;
    }

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public OrderListViewModel ViewModel { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        ViewData["ActivePage"] = ManageNavPages.Orders;

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound();

        const int PageSize = 5;

        var allUserOrders = await _orderRepository.GetAllAsync();
        var filtered = allUserOrders
            .Where(o => o.UserId == user.Id &&
                        (string.IsNullOrWhiteSpace(SearchTerm) ||
                         o.Id.ToString().Contains(SearchTerm) ||
                         o.OrderDetails.Any(d => d.Product.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))))
            .ToList();

        var pagedOrders = filtered
            .Skip((PageNumber - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        ViewModel = new OrderListViewModel
        {
            Items = pagedOrders,
            TotalRecords = filtered.Count,
            PageNumber = PageNumber,
            PageSize = PageSize,
            SearchTerm = SearchTerm
        };

        return Page();
    }

    public async Task<IActionResult> OnPostCancelAsync(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var order = (await _orderRepository.GetAllAsync())
            .FirstOrDefault(o => o.Id == id && o.UserId == user.Id);

        if (order == null)
        {
            TempData["Error"] = "Không tìm thấy đơn hàng.";
            return RedirectToPage();
        }

        if (order.Status == "Đang giao hàng" || order.Status == "Đã hoàn thành" || order.Status == "Đã hủy")
        {
            TempData["Error"] = "Đơn hàng đã được xử lý và không thể hủy.";
            return RedirectToPage();
        }

        order.Status = "Đã hủy";

        foreach (var detail in order.OrderDetails)
        {
            var variant = detail.Product?.Variants?
                .FirstOrDefault(v => v.Color == detail.Color && v.Size == detail.Size);
            if (variant != null)
            {
                variant.StockQuantity += detail.Quantity;
            }
        }

        await _orderRepository.UpdateAsync(order);
        TempData["Success"] = $"Đơn hàng #{id} đã được hủy.";

        return RedirectToPage(new { PageNumber, SearchTerm });
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