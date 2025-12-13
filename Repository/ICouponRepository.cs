using CuaHangBanSach.Models;
using CuaHangBanSach.ViewModels;

namespace CuaHangBanSach.Repository
{
    public interface ICouponRepository
    {
        Task<IEnumerable<Coupon>> GetAllAsync();
        Task<Coupon?> GetByIdAsync(int id);
        Task<Coupon?> GetByCodeAsync(string code);
        Task AddAsync(Coupon coupon);
        Task UpdateAsync(Coupon coupon);
        Task DeleteAsync(int id);
        Task SaveAsync();
        Task<CouponListViewModel> GetPagedCouponsAsync(string? search, int page, int pageSize);
    }
}