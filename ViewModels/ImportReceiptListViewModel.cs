using CuaHangBanSach.Models;

namespace CuaHangBanSach.ViewModels
{
    public class ImportReceiptListViewModel : Paginate<ImportReceipt>
    {
        public string? SearchTerm { get; set; }
    }
}