
using System;
using System.Collections.Generic;
using OrderMicroservice.Common.Enums;

namespace OrderMicroservice.Domain
{
    public class Order
    {
        public int Id {get; set;}
        public OrderSource Source { get; set; }          // "WEB" | "VENDOR"
        public required string Channel { get; set; }    // e.g., "RETAIL" (kept as string per your data)
        public int VendorId { get; set; }
        public required string Currency { get; set; }
        public required string OrderNumber { get; set; } 
        public DateTime PlacedAt { get; set; }

        public OrderPriority OrderPriority { get; set; }     // LOW | NORMAL | HIGH | URGENT
        public ShippingPriority ShippingPriority { get; set; } // ECONOMY | STANDARD | EXPEDITED | OVERNIGHT

        public Customer? Customer { get; set; }
        public List<OrderItem> Items { get; set; } = new();

        public required ShippingDetail Shipping { get; set; } 
    }
}
