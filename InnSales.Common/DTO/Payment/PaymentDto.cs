using InnSales.Common.Enums;

public class PaymentDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string ReadableOrderId { get; set; }

    public string CardHolderName { get; set; }
    public string CardNumber { get; set; }
    public string CardExpiryDate { get; set; }

    public decimal AmountPaid { get; set; }
    public DateTime PaymentDate { get; set; }
    public TimeSpan TransactionTime { get; set; }

    public string TransactionId { get; set; }
    public PaymentStatus PaymentStatus { get; set; } // Enum

    public bool IsRefunded { get; set; }
    public DateTime? RefundDate { get; set; }
}
