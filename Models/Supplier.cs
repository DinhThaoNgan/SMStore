using System.ComponentModel.DataAnnotations;

namespace CuaHangBanSach.Models
{
    public class Supplier
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; }
        
        public string? Address { get; set; }
        
        [Phone]
        public string? PhoneNumber { get; set; }
        
        [EmailAddress]
        public string? Email { get; set; }
        
        public string? ContactPerson { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        // Navigation property
        public List<ImportReceipt> ImportReceipts { get; set; } = new List<ImportReceipt>();
    }
}