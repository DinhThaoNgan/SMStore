using CuaHangBanSach.Models;

namespace CuaHangBanSach.Repository
{
    public interface IOrderDetailRepository
    {
        Task<IEnumerable<OrderDetail>> GetAllAsync();
        Task<IEnumerable<OrderDetail>> GetTopSellingProductsAsync(int topN);
        Task<decimal> GetProductRevenueAsync(int productId);
    }
}