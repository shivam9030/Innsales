using InnSales.Common.Enums;

namespace InnSales.Domain.Entities
{
    public class Payment
    {
        public Guid Id { get; set; }

        // Link to Order
        public Guid OrderId { get; set; }
        public string ReadableOrderId { get; set; }

        // Payment Info
        public string TransactionId { get; set; }
        public string CardHolderName { get; set; }
        public string MaskedCardNumber { get; set; }
        public string CardExpiryDate { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public decimal AmountPaid { get; set; }
        public DateTime PaymentDate { get; set; }
        public TimeSpan TransactionTime { get; set; }

        // Refund Info
        public bool IsRefunded { get; set; } = false;
        public DateTime? RefundDate { get; set; }

        // Navigation Property
        public Order Order { get; set; }
    }
}
