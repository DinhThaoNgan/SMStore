namespace CuaHangBanSach.Models
{
    public class OrderDetail
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public int ProductId { get; set; }

        public int Quantity { get; set; }

        public decimal Price { get; set; }

        // Thông tin biến thể
        public string? Color { get; set; }

        public string? ImageUrl { get; set; }

        public string? Size { get; set; }

        public Order Order { get; set; }

        public Product Product { get; set; }
    }
}