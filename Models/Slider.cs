using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace CuaHangBanSach.Models
{
    public class Slider
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Yêu cầu không được bỏ trống tên slider")]
        public string Name { get; set; }

        public string? Image { get; set; }

        public string? Description { get; set; }

        [NotMapped]
        public IFormFile? ImageUpload { get; set; }
    }
}