public class CartItem
{
    public int ProductId { get; set; }
    public string? Name { get; set; }
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }

    // Với sản phẩm có biến thể
    public string? Color { get; set; }
    public string? Size { get; set; }
}