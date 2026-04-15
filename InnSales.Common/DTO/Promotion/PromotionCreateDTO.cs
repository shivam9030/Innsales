using InnSales.Common.Enums;
namespace InnSales.Common.DTO
{
public class PromotionCreateDto
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public int Quantity { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal MinimumOrderValue { get; set; }
    public decimal PromotionValue { get; set; }
    public DiscountType DiscountType { get; set; } // e.g., "Percentage", "Flat"
    public PromotionStatus Status { get; set; } // e.g., "Active", "Inactive"
}
}