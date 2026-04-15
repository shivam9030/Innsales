
namespace InnSales.Common.DTO
{
public class AddBasketItemDto
{
    public string? UserId { get; set; }
        public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}}