using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CuaHangBanSach.Models;
using CuaHangBanSach.Repository;

namespace CuaHangBanSach.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IProductRepository _productRepository;

        public OrdersController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IProductRepository productRepository)
        {
            _context = context;
            _userManager = userManager;
            _productRepository = productRepository;
        }

        // Hiển thị form checkout
        public async Task<IActionResult> Checkout()
        {
            var user = await _userManager.GetUserAsync(User);
            ViewBag.User = user;
            
            // Trong subtask này, chúng ta chỉ cần implement cơ bản
            // Chi tiết sẽ được thêm trong các subtask sau
            
            return View(new Order());
        }

        // Xử lý checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(Order order)
        {
            // Trong subtask này, chúng ta chỉ cần implement cơ bản
            // Chi tiết sẽ được thêm trong các subtask sau
            
            // Tạo một order mẫu để hiển thị trong view
            var sampleOrder = new Order
            {
                Id = 12345,
                OrderDate = DateTime.UtcNow,
                TotalPrice = 1000000,
                Status = "Pending",
                CustomerName = "Nguyễn Văn A",
                PhoneNumber = "0123456789",
                ShippingAddress = "123 Đường ABC, Quận XYZ, TP HCM",
                Notes = "Giao hàng trong giờ hành chính",
                CouponCode = "DISCOUNT10",
                DiscountAmount = 100000,
                OrderDetails = new List<OrderDetail>
                {
                    new OrderDetail
                    {
                        Id = 1,
                        ProductId = 1,
                        Quantity = 2,
                        Price = 500000,
                        Color = "Đỏ",
                        Size = "L",
                        Product = new Product { Name = "Sản phẩm mẫu 1", ImageUrl = "/images/product1.jpg" }
                    },
                    new OrderDetail
                    {
                        Id = 2,
                        ProductId = 2,
                        Quantity = 1,
                        Price = 300000,
                        Color = "Xanh",
                        Size = "M",
                        Product = new Product { Name = "Sản phẩm mẫu 2", ImageUrl = "/images/product2.jpg" }
                    }
                }
            };
            
            return View("OrderCompleted", sampleOrder);
        }

        // Hiển thị đơn hàng đã hoàn thành
        public IActionResult OrderCompleted(Order order)
        {
            return View(order);
        }
    }
}