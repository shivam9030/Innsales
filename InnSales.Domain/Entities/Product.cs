using System;

namespace InnSales.Domain.Entities
{
    public class Product
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public decimal Price { get; set; }
        public int? StockQuantity { get; set; }
        public bool IsDeleted { get; set; } = false;

        public Guid CategoryId { get; set; }
        public Category? Category { get; set; } = null!;
        
        public bool IsPromoProduct { get; set; } = false;

    public string? AvailabilityStatus => 
    IsPromoProduct ? null : (StockQuantity == 0 ? "Out of Stock" : "In Stock");

    }
}