namespace InnSales.Common.DTO
{
    public class OrderItemDto
    {
        public Guid OrderId { get; set; }                    // Parent Order ID reference
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string ImageUrl { get; set; }

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal EffectivePrice { get; set; } // Price after applying promo code discount, if anyz
         public bool IsPromoProduct { get; set; }
    }
}
