using CuaHangBanSach.Models;

namespace CuaHangBanSach.ViewModels
{
    public class BrandListViewModel : Paginate<Brand>
    {
        public string? SearchTerm { get; set; }
    }
}