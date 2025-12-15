using CuaHangBanSach.Models;

namespace CuaHangBanSach.Repository
{
    public interface ISupplierRepository
     {
        Task<List<Supplier>> GetAllAsync(string? searchTerm = null);
        Task<Supplier?> GetByIdAsync(int id);
        Task AddAsync(Supplier supplier);
        Task UpdateAsync(Supplier supplier);
        Task DeleteAsync(int id);
    }
}