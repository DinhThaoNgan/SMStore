using Microsoft.EntityFrameworkCore;
using CuaHangBanSach.Models;
using CuaHangBanSach.ViewModels;

namespace CuaHangBanSach.Repository
{
    public class CouponRepository : ICouponRepository
    {
        private readonly ApplicationDbContext _context;

        public CouponRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Coupon>> GetAllAsync()
        {
            return await _context.Coupons.ToListAsync();
        }

        public async Task<Coupon?> GetByIdAsync(int id)
        {
            return await _context.Coupons.FindAsync(id);
        }

        public async Task AddAsync(Coupon coupon)
        {
            _context.Coupons.Add(coupon);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Coupon coupon)
        {
            _context.Coupons.Update(coupon);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon != null)
            {
                _context.Coupons.Remove(coupon);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Coupon?> GetByCodeAsync(string code)
        {
            return await _context.Coupons
                .FirstOrDefaultAsync(c => c.Code.ToUpper() == code.ToUpper());
        }
        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }


        // ✅ Phân trang + tìm kiếm
        public async Task<CouponListViewModel> GetPagedCouponsAsync(string? search, int page, int pageSize)
        {
            var query = _context.Coupons.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c => c.Code.Contains(search));
            }

            var totalRecords = await query.CountAsync();
            var items = await query
                .OrderByDescending(c => c.StartDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new CouponListViewModel
            {
                Items = items,
                PageNumber = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                SearchTerm = search
            };
        }
    }
}