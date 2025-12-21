using CuaHangBanSach.Models;

namespace CuaHangBanSach.ViewModels
{
    public class CategoryListViewModel : Paginate<Category>
    {
        public string? SearchTerm { get; set; }
    }
}