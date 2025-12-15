using System.ComponentModel.DataAnnotations;

namespace CuaHangBanSach.Models
{
    public class Supplier
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [Phone]
        public string? Phone { get; set; }

        public string? Address { get; set; }

        // Navigation
        public List<ImportReceipt> ImportReceipts { get; set; } = new();
    }
}