namespace WebAppBowling.Models
{
    // товар в корзине (корзина хранится в сессии, в базу не пишется)
    public class CartItem
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Total => Price * Quantity;
    }
}
