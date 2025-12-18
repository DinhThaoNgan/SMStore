using Microsoft.EntityFrameworkCore;
using CuaHangBanSach.Models;
using CuaHangBanSach.ViewModels;

namespace CuaHangBanSach.Repository
{
    public interface IImportReceiptRepository
    {
        Task<Paginate<ImportReceipt>> GetPagedReceiptsAsync(string? searchTerm, int page, int pageSize);
        Task<ImportReceipt?> GetByIdAsync(int id);
        Task AddAsync(ImportReceipt receipt);
        Task UpdateAsync(ImportReceipt receipt);
        Task DeleteAsync(int id);
        Task<List<(DateTime Date, decimal TotalCost)>> GetImportCostsByDay(DateTime from, DateTime to);
        Task<List<(int Month, int Year, decimal TotalCost)>> GetImportCostsByMonth(DateTime from, DateTime to);
        Task<List<(int Year, decimal TotalCost)>> GetImportCostsByYear(int fromYear, int toYear);
    }
}