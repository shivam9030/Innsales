namespace InnSales.Services
{
    public interface IPaymentTimeoutService
    {
        Task CheckAndCancelTimedOutPaymentsAsync();
    }
}