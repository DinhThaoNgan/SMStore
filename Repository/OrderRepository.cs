using Microsoft.EntityFrameworkCore;
using CuaHangBanSach.Models;

namespace CuaHangBanSach.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext _context;

        public OrderRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            return await _context.Orders
                .Include(o => o.ApplicationUser)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .ThenInclude(p => p.Variants)
                .ToListAsync();
        }

        public async Task<Order?> GetByIdAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.ApplicationUser)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task DeleteAsync(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateAsync(Order order)
        {
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
        }

        public async Task<decimal> GetTotalRevenueAsync()
        {
            return await _context.Orders
                .Where(o => o.Status == "Đã giao")
                .SumAsync(o => o.TotalPrice);
        }

        public async Task<decimal> GetRevenueByMonthAsync(int month, int year)
        {
            return await _context.Orders
                .Where(o => o.Status == "Đã giao" && o.OrderDate.Month == month && o.OrderDate.Year == year)
                .SumAsync(o => o.TotalPrice);
        }

        public async Task<decimal> GetRevenueByDateRangeAsync(DateTime start, DateTime end)
        {
            return await _context.Orders
                .Where(o => o.Status == "Đã giao" && o.OrderDate >= start && o.OrderDate <= end)
                .SumAsync(o => o.TotalPrice);
        }

        public async Task<(List<Order> Orders, int TotalRecords)> GetPagedOrdersAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var query = _context.Orders
                .Include(o => o.ApplicationUser)
                .Include(o => o.OrderDetails)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(o => o.ApplicationUser.FullName.Contains(searchTerm) ||
                                         o.ApplicationUser.UserName.Contains(searchTerm));
            }

            var totalRecords = await query.CountAsync();

            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (orders, totalRecords);
        }

        public Order GetOrderById(int orderId)
        {
            return _context.Orders
                .Include(o => o.ApplicationUser)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefault(o => o.Id == orderId);
        }
    }
}