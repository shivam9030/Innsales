
// IPaymentTransactionPublisher.cs
using System.Threading.Tasks;
using InnSales.Common.DTO;

namespace InnSales.Services
{
    public interface IPaymentTransactionPublisher
    {
        Task PublishAsync(PaymentTransactionMessage message);
    }
}