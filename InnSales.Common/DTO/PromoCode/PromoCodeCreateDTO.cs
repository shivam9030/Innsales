namespace InnSales.Common.DTO
{
public class PromoCodeCreateDto
{
    public Guid PromotionId { get; set; }
    public string Code { get; set; }
    public bool IsUniqueCode { get; set; }
    public int MaxUsageLimit { get; set; }
}
}