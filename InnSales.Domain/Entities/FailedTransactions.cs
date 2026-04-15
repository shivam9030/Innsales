public class FailedTransaction
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string TransactionId { get; set; }
    public string FailureMessage { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public string Description { get; set; }
    public string CardHolderName { get; set; }
    public DateTime LoggedDate { get; set; }
}