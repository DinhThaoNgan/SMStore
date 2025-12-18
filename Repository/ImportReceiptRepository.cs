using Microsoft.EntityFrameworkCore;
using CuaHangBanSach.Models;
using CuaHangBanSach.ViewModels;

namespace CuaHangBanSach.Repository
{
    public class ImportReceiptRepository : IImportReceiptRepository
    {
        private readonly ApplicationDbContext _context;

        public ImportReceiptRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Paginate<ImportReceipt>> GetPagedReceiptsAsync(string? searchTerm, int page, int pageSize)
        {
            var query = _context.ImportReceipts
                .Include(r => r.Supplier)
                .Include(r => r.Details)
                .ThenInclude(d => d.ProductVariant)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(r => r.Supplier.Name.Contains(searchTerm));
            }

            var total = await query.CountAsync();
            var items = await query.OrderByDescending(r => r.ImportDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new Paginate<ImportReceipt>
            {
                Items = items,
                PageNumber = page,
                PageSize = pageSize,
                TotalRecords = total
            };
        }

        public async Task<ImportReceipt?> GetByIdAsync(int id)
        {
            return await _context.ImportReceipts
                .Include(r => r.Supplier)
                .Include(r => r.Details)
                    .ThenInclude(d => d.ProductVariant)
                        .ThenInclude(v => v.Product)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task AddAsync(ImportReceipt receipt)
        {
            Console.WriteLine("ImportReceipt - SupplierId: " + receipt.SupplierId);
            Console.WriteLine("Details count: " + receipt.Details.Count);
            foreach (var d in receipt.Details)
            {
                Console.WriteLine($" - VariantId: {d.ProductVariantId}, Qty: {d.Quantity}, Price: {d.UnitPrice}");
            }

            await _context.ImportReceipts.AddAsync(receipt);
            await _context.SaveChangesAsync();
        }


        public async Task UpdateAsync(ImportReceipt receipt)
        {
            _context.ImportReceipts.Update(receipt);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var receipt = await _context.ImportReceipts.FindAsync(id);
            if (receipt != null)
            {
                _context.ImportReceipts.Remove(receipt);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<(DateTime Date, decimal TotalCost)>> GetImportCostsByDay(DateTime from, DateTime to)
        {
            return await _context.ImportReceipts
                .Where(r => r.Status == "Đã hoàn thành" && r.ImportDate.Date >= from && r.ImportDate.Date <= to)
                .GroupBy(r => r.ImportDate.Date)
                .Select(g => new ValueTuple<DateTime, decimal>(g.Key, g.Sum(r => r.TotalAmount)))
                .ToListAsync();
        }

        public async Task<List<(int Month, int Year, decimal TotalCost)>> GetImportCostsByMonth(DateTime from, DateTime to)
        {
            return await _context.ImportReceipts
                .Where(r => r.Status == "Đã hoàn thành" && r.ImportDate >= from && r.ImportDate <= to)
                .GroupBy(r => new { r.ImportDate.Month, r.ImportDate.Year })
                .Select(g => new ValueTuple<int, int, decimal>(g.Key.Month, g.Key.Year, g.Sum(r => r.TotalAmount)))
                .ToListAsync();
        }

        public async Task<List<(int Year, decimal TotalCost)>> GetImportCostsByYear(int fromYear, int toYear)
        {
            return await _context.ImportReceipts
                .Where(r => r.Status == "Đã hoàn thành" && r.ImportDate.Year >= fromYear && r.ImportDate.Year <= toYear)
                .GroupBy(r => r.ImportDate.Year)
                .Select(g => new ValueTuple<int, decimal>(g.Key, g.Sum(r => r.TotalAmount)))
                .ToListAsync();
        }

    }
}