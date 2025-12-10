using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CuaHangBanSach.Extensions;
using CuaHangBanSach.Models;
using CuaHangBanSach.Repository;

namespace CuaHangBanSach.Controllers
{
    [Authorize]
    public class ShoppingCartController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ShoppingCartController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IProductRepository productRepository)
        {
            _context = context;
            _userManager = userManager;
            _productRepository = productRepository;
        }

        // ✅ Thêm sản phẩm vào giỏ
        public async Task<IActionResult> AddToCart(int productId, int quantity, string? color, string? size)
        {
            var product = await _productRepository.GetByIdWithVariantsAsync(productId);
            if (product == null) return NotFound();

            var variant = product.Variants.FirstOrDefault(v => v.Color == color && v.Size == size);
            if (variant == null) return NotFound();

            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            var existingQuantity = cart.Items
                .Where(i => i.ProductId == productId && i.Color == color && i.Size == size)
                .Sum(i => i.Quantity);

            if (existingQuantity + quantity > variant.StockQuantity)
            {
                TempData["Error"] = $"Sản phẩm chỉ còn {variant.StockQuantity - existingQuantity} trong kho!";
                return RedirectToAction("Index");
            }

            var cartItem = new CartItem
            {
                ProductId = product.Id,
                Name = product.Name,
                Price = product.Price,
                Quantity = quantity,
                Color = color,
                Size = size,
                ImageUrl = variant.ImageUrl ?? product.ImageUrl
            };

            cart.AddItem(cartItem);
            HttpContext.Session.SetObjectAsJson("Cart", cart);
            return RedirectToAction("Index");
        }

        // ✅ API Ajax
        [HttpPost]
        public async Task<IActionResult> AddToCartWithVariant([FromBody] CartItem item)
        {
            var product = await _productRepository.GetByIdWithVariantsAsync(item.ProductId);
            if (product == null) return NotFound();

            var variant = product.Variants.FirstOrDefault(v => v.Color == item.Color && v.Size == item.Size);
            if (variant == null) return NotFound();

            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            var existingQuantity = cart.Items
                .Where(i => i.ProductId == item.ProductId && i.Color == item.Color && i.Size == item.Size)
                .Sum(i => i.Quantity);

            if (existingQuantity + item.Quantity > variant.StockQuantity)
            {
                return BadRequest(new { error = $"Sản phẩm chỉ còn {variant.StockQuantity - existingQuantity} trong kho!" });
            }

            item.Name = product.Name;
            item.Price = product.Price;
            item.ImageUrl = variant.ImageUrl ?? product.ImageUrl;

            cart.AddItem(item);
            HttpContext.Session.SetObjectAsJson("Cart", cart);

            return Ok();
        }

        // ✅ Giỏ hàng
        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            return View(cart);
        }

        // ✅ Xóa 1 item
        public IActionResult RemoveFromCart(int productId, string? color, string? size)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart != null)
            {
                cart.RemoveItem(productId, color, size);
                HttpContext.Session.SetObjectAsJson("Cart", cart);
            }
            return RedirectToAction("Index");
        }

        // ✅ Cập nhật số lượng
        public IActionResult UpdateQuantity(int productId, string? color, string? size, int quantity)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart != null)
            {
                var product = _productRepository.GetByIdWithVariantsAsync(productId).Result;
                var variant = product?.Variants.FirstOrDefault(v => v.Color == color && v.Size == size);

                if (variant != null && quantity > variant.StockQuantity)
                {
                    TempData["Error"] = $"Tối đa chỉ có {variant.StockQuantity} sản phẩm trong kho.";
                    return RedirectToAction("Index");
                }

                cart.UpdateQuantity(productId, color, size, quantity);
                HttpContext.Session.SetObjectAsJson("Cart", cart);
            }
            return RedirectToAction("Index");
        }

        // ✅ Xóa toàn bộ giỏ
        public IActionResult ClearCart()
        {
            HttpContext.Session.Remove("Cart");
            return RedirectToAction("Index");
        }

        // ✅ Tổng số lượng
        [HttpGet]
        public IActionResult Quantity()
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            var total = cart?.Items.Sum(i => i.Quantity) ?? 0;
            return Json(total);
        }

        // ✅ Checkout GET
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var user = await _userManager.GetUserAsync(User);
            ViewBag.User = user;
            return View(new Order());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(IFormCollection form)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart == null || !cart.Items.Any())
                return RedirectToAction("Index");

            var user = await _userManager.GetUserAsync(User);

            // ✅ Kiểm tra tick
            bool useDefault = form["useDefaultInfo"] == "on";

            // ✅ Lấy thông tin
            string customerName = useDefault ? user.FullName : form["CustomerName"].ToString().Trim();
            string phone = useDefault ? user.PhoneNumber : form["PhoneNumber"].ToString().Trim();
            string address = useDefault ? user.Address : form["ShippingAddress"].ToString().Trim();
            string notes = form["Notes"].ToString();
            string couponCode = form["CouponCode"].ToString().Trim();

            if (!form.ContainsKey("codCheck"))
            {
                TempData["Error"] = "Bạn phải đồng ý thanh toán khi nhận hàng.";
                return RedirectToAction("Checkout");
            }

            // ✅ Tính tổng
            var total = cart.Items.Sum(i => i.Price * i.Quantity);
            decimal discountValue = 0;
            Coupon? coupon = null;

            if (!string.IsNullOrEmpty(couponCode))
            {
                coupon = await _context.Coupons
                    .Where(c => c.Code == couponCode &&
                                DateTime.Now >= c.StartDate &&
                                DateTime.Now <= c.EndDate)
                    .FirstOrDefaultAsync();

                if (coupon != null)
                {
                    discountValue = coupon.IsPercentage
                        ? total * coupon.DiscountAmount / 100
                        : coupon.DiscountAmount;
                }
            }

            // ✅ Tạo đơn hàng
            var order = new Order
            {
                CustomerName = customerName,
                PhoneNumber = phone,
                ShippingAddress = address,
                Notes = notes,
                CouponCode = coupon?.Code,
                DiscountAmount = discountValue,
                TotalPrice = total - discountValue,
                UserId = user.Id,
                OrderDate = DateTime.UtcNow,
                OrderDetails = new List<OrderDetail>()
            };

            // ✅ Check tồn kho
            foreach (var item in cart.Items)
            {
                var variant = await _context.ProductVariants
                    .FirstOrDefaultAsync(v => v.ProductId == item.ProductId &&
                                              v.Color == item.Color &&
                                              v.Size == item.Size);

                if (variant == null || item.Quantity > variant.StockQuantity)
                {
                    TempData["Error"] = $"Không đủ tồn kho cho {item.Name} ({item.Color}, {item.Size}).";
                    return RedirectToAction("Index");
                }

                variant.StockQuantity -= item.Quantity;

                order.OrderDetails.Add(new OrderDetail
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    Color = item.Color,
                    Size = item.Size,
                    ImageUrl = item.ImageUrl
                });
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            HttpContext.Session.Remove("Cart");

            return View("OrderCompleted", order.Id);
        }


        // ✅ Lấy số lượng variant
        [HttpGet]
        public IActionResult GetVariantQuantityInCart(int productId, string color, string size)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            var item = cart?.Items.FirstOrDefault(i => i.ProductId == productId && i.Color == color && i.Size == size);
            return Json(item?.Quantity ?? 0);
        }
    }
}
