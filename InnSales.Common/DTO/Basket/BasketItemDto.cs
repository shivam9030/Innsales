namespace InnSales.Common.DTO
{
    public class BasketItemDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string ImageUrl { get; set; }
       public decimal Price { get; set; } // Original price
     public decimal EffectivePrice { get; set; } // Discounted price after promo

        public int Quantity { get; set; }
    }
}