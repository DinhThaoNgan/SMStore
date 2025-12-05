using System.ComponentModel.DataAnnotations;

namespace CuaHangBanSach.Models
{
    public class Brand
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên thương hiệu không được để trống")]
        [StringLength(100)]
        public string Name { get; set; }

        public List<Product>? Products { get; set; }
    }
}