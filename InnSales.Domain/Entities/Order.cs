using System;
using System.Collections.Generic;
using InnSales.Domain.UserManagement.Entities;
using InnSales.Domain.Entities;
using InnSales.Common.Enums;

namespace InnSales.Domain.Entities
{
    public class Order
    {
        public Guid Id { get; set; }
        public string ReadableOrderId { get; set; }

        public string CustomerId { get; set; }
        public ApplicationUser Customer { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public DateTime? RequiredByDate { get; set; }

        public OrderStatus OrderStatus { get; set; } = OrderStatus.Pending;
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

        public decimal Tax { get; set; }
        public decimal TotalAmount { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        // Payment Info (after successful payment)
        public string? TransactionId { get; set; }         // Razorpay payment ID
        public string? CardHolderName { get; set; }        // Name on card
        public string? MaskedCardNumber { get; set; }      // e.g., **** **** **** 1234
        public string? CardExpiryDate { get; set; }        // Format: MM/YY or MM/YYYY
    }
}
