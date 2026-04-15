namespace InnSales.Common.DTO
{
public class PromoCodeUpdateDto
{
    public Guid PromoCodeId { get; set; } // Required for update
    public int MaxUsageLimit { get; set; } // Optional for partial update
    public bool IsUniqueCode { get; set; }
}
}