using InnSales.Common.Enums;
using InnSales.Common.DTO;

public class OrderDto
{
    public Guid Id { get; set; }
    public string ReadableOrderId { get; set; }

    public string CustomerId { get; set; }
    public string CustomerName { get; set; } // Optional, if you want to show name

    public DateTime OrderDate { get; set; }
    public DateTime? RequiredByDate { get; set; }

    public OrderStatus OrderStatus { get; set; }
    public PaymentStatus PaymentStatus { get; set; }

    public decimal Tax { get; set; }
    public decimal TotalAmount { get; set; }

    public List<OrderItemDto> OrderItems { get; set; }

    // Payment Info (only populated if PaymentStatus == Success)
    public string? TransactionId { get; set; }
    public string? CardHolderName { get; set; }
    public string? MaskedCardNumber { get; set; }
    public string? CardExpiryDate { get; set; }
    public decimal Subtotal { get; set; }   // Item total before discount
public decimal Discount { get; set; }   // Promo discount amount
  public string? PaymentToken { get; set; }
}
