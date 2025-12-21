using CuaHangBanSach.Models;

namespace CuaHangBanSach.ViewModels
{
    public class UserListViewModel : Paginate<ApplicationUser>
    {
        public string? SearchTerm { get; set; }
        public Dictionary<string, string> UserRoles { get; set; } = new();
    }
}