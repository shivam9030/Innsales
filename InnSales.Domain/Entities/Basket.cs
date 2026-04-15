using System;

namespace InnSales.Domain.Entities
{
    public class BasketItem
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public Product Product { get; set; }
        public Guid? PromoCodeId { get; set; }
        public PromoCode PromoCode { get; set; }
    }
}