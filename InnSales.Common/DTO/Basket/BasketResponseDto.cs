
namespace InnSales.Common.DTO{
public class BasketResponseDto
{
    public List<BasketItemDto> Items { get; set; }
    public decimal TotalPrice { get; set; } // Original basket total
    public decimal DiscountedTotal { get; set; } // Basket total after discount
    public decimal DiscountAmount{get;set;} // Optional
}
}
