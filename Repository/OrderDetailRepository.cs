using Microsoft.EntityFrameworkCore;
using CuaHangBanSach.Models;

namespace CuaHangBanSach.Repository
{
    public class OrderDetailRepository : IOrderDetailRepository
    {
        private readonly ApplicationDbContext _context;

        public OrderDetailRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<OrderDetail>> GetAllAsync()
        {
            return await _context.OrderDetails
                .Include(od => od.Product)
                .Include(od => od.Order)
                .ToListAsync();
        }

        public async Task<IEnumerable<OrderDetail>> GetTopSellingProductsAsync(int topN)
        {
            return await _context.OrderDetails
                .GroupBy(od => od.ProductId)
                .Select(g => new OrderDetail
                {
                    ProductId = g.Key,
                    Quantity = g.Sum(x => x.Quantity),
                    Price = g.Sum(x => x.Price * x.Quantity)
                })
                .OrderByDescending(od => od.Quantity)
                .Take(topN)
                .ToListAsync();
        }

        public async Task<decimal> GetProductRevenueAsync(int productId)
        {
            return await _context.OrderDetails
                .Where(od => od.ProductId == productId)
                .SumAsync(od => od.Price * od.Quantity);
        }
    }
}