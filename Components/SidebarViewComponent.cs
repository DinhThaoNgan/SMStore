using Microsoft.AspNetCore.Mvc;
using CuaHangBanSach.Models;
using CuaHangBanSach.Repository;

namespace CuaHangBanSach.Components
{
    public class SidebarViewComponent : ViewComponent
    {
        private readonly ICategoryRepository _categoryRepository;

        public SidebarViewComponent(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return View(categories);
        }
    }
}