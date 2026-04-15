namespace InnSales.Domain.Entities
{
    public class PromoCode
    {
        public Guid PromoCodeId { get; set; }
        public Guid PromotionId { get; set; }
        public string Code { get; set; }
        public bool isUniqueCode { get; set; }
        public int MaxUsageLimit { get; set; }
        public int UsageCount { get; set; }=0;
        public DateTime? UsedOn { get; set; }
        public DateTime CreatedAt { get; set; }
        public Promotion Promotion { get; set; }
    }
}