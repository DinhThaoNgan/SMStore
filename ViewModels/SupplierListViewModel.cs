using CuaHangBanSach.Models;

namespace CuaHangBanSach.ViewModels
{
    public class SupplierListViewModel : Paginate<Supplier>
    {
        public string? SearchTerm { get; set; }
    }
}