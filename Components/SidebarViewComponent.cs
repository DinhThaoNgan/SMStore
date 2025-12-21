using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CuaHangBanSach.Models;

namespace CuaHangBanSach.Components
{
    public class SidebarViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        public SidebarViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var categories = await _context.Categories.ToListAsync();
            var brands = await _context.Brands.ToListAsync();
            return View((categories, brands));
        }

    }
}