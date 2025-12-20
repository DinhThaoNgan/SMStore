using Microsoft.AspNetCore.Mvc;
using CuaHangBanSach.Models;
using CuaHangBanSach.Extensions;

namespace CuaHangBanSach.ViewComponents
{
    public class ShoppingCartViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            int totalItems = cart.GetTotalQuantity();
            return View(totalItems);
        }
    }
}