using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CuaHangBanSach.Models;

namespace CuaHangBanSach.ViewComponents
{
    public class UserRoleViewComponent : ViewComponent
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserRoleViewComponent(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            return View("Default", roles);
        }
    }
}