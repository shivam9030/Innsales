using System;

namespace InnSales.Common.DTO
{
    public class PaymentTransactionMessage
    {
        // MUST match Order.Id type
        public Guid OrderId { get; set; }

        // Outcome
        public bool Success { get; set; }

        // Stripe
        public string? TransactionId { get; set; }

        // Card info (matches Order entity)
        public string? CardHolderName { get; set; }
        public string? MaskedCardNumber { get; set; }
        public string? CardExpiryDate { get; set; }

        // Failure
        public string? FailureReason { get; set; }

        // Audit
        public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
    }
}
