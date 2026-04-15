namespace InnSales.Common.DTO
{
public class PromoCodeResponseDto
{
    public Guid PromoCodeId { get; set; }
    public string Code { get; set; }
    public bool IsUniqueCode { get; set; }
    public int MaxUsageLimit { get; set; }
    public int UsageCount { get; set; }
    public DateTime? UsedOn { get; set; }
    public DateTime CreatedAt { get; set; }
}
}