using System;
using System.Collections.Generic;
using InnSales.Domain.Entities;

namespace InnSales.Domain.Entities
{
    public class Category
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }

        public bool IsDeleted { get; set; } = false;

        // Navigation property to related products
        public List<Product> Products { get; set; } = new();
    }
}