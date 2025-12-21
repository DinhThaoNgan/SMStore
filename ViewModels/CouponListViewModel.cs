using CuaHangBanSach.Models;

namespace CuaHangBanSach.ViewModels
{
    public class CouponListViewModel : Paginate<Coupon>
    {
        public string? SearchTerm { get; set; }
    }
}