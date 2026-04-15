using System;
using InnSales.Common.Enums;

namespace InnSales.Domain.Entities
{
    public class Promotion
    {
        public Guid PromotionId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public int Quantity { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal minimumOrderValue { get; set; } 
        public decimal PromotionValue { get; set; }
        public DiscountType DiscountType { get; set; }
        public PromotionStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public ICollection<PromoCode> PromoCodes { get; set; }
    }
}