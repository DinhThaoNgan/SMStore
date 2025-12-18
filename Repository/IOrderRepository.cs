using CuaHangBanSach.Models;

namespace CuaHangBanSach.Repository
{
    public interface IOrderRepository
    {
        Task<IEnumerable<Order>> GetAllAsync();
        Task<Order?> GetByIdAsync(int id);
        Task DeleteAsync(int id);
        Task UpdateAsync(Order order);
        Order GetOrderById(int orderId);
        Task<decimal> GetTotalRevenueAsync();
        Task<decimal> GetRevenueByMonthAsync(int month, int year);
        Task<decimal> GetRevenueByDateRangeAsync(DateTime start, DateTime end);
        Task<(List<Order> Orders, int TotalRecords)> GetPagedOrdersAsync(int pageNumber, int pageSize, string? searchTerm);
    }
}