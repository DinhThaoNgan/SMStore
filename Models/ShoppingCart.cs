using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace CuaHangBanSach.Models
{
    public class ShoppingCart
    {
        public List<CartItem> Items { get; set; } = new List<CartItem>();

        public void AddItem(CartItem item)
        {
            var existingItem = Items.FirstOrDefault(i =>
                i.ProductId == item.ProductId &&
                i.Color == item.Color &&
                i.Size == item.Size);

            if (existingItem != null)
            {
                existingItem.Quantity += item.Quantity;
            }
            else
            {
                Items.Add(item);
            }
        }

        public void RemoveItem(int productId, string? color, string? size)
        {
            var itemToRemove = Items.FirstOrDefault(i =>
                i.ProductId == productId &&
                i.Color == color &&
                i.Size == size);
            if (itemToRemove != null)
            {
                Items.Remove(itemToRemove);
            }
        }

        public void UpdateQuantity(int productId, string? color, string? size, int quantity)
        {
            var item = Items.FirstOrDefault(i =>
                i.ProductId == productId &&
                i.Color == color &&
                i.Size == size);
            if (item != null)
            {
                item.Quantity = quantity;
            }
        }

        public int GetTotalQuantity()
        {
            return Items.Sum(i => i.Quantity);
        }

        public decimal GetTotalPrice()
        {
            return Items.Sum(i => i.Price * i.Quantity);
        }

        public void Clear()
        {
            Items.Clear();
        }
    }
}