using CuaHangBanSach.Models;

namespace CuaHangBanSach.ViewModels
{
    public class OrderListViewModel : Paginate<Order>
    {
        public string? SearchTerm { get; set; }
    }
}