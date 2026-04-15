namespace InnSales.Common.DTO
{
    public class ProductDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public decimal Price { get; set; }
        public int? StockQuantity { get; set; }
        public Guid CategoryId { get; set; }
        public bool IsPromoProduct { get; set; }
        // New: Availability status for frontend
        public string AvailabilityStatus { get; set; }
    }
}
