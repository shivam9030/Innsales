namespace InnSales.Common.DTO
{
    public class StripePaymentRequestDto
    {
        public Guid OrderId { get; set; }
        public string Token { get; set; } // Stripe token from frontend
        public string Currency { get; set; }
        public string CardHolderName { get; set; }
         public string PaymentToken { get; set; }
    }

    public class StripePaymentResponseDto
    {
        public bool Success { get; set; }
        public string TransactionId { get; set; }
        public string MaskedCardNumber { get; set; }
        public string CardExpiryDate { get; set; }
        public string Message { get; set; }
    }
}
