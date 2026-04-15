
// Messaging/IOrderPublisher.cs
using System.Threading.Tasks;
using OrderMicroservice.Common.DTO;

namespace OrderMicroservice.Service
{
    public interface IOrderPublisher
    {
        Task PublishAsync(OrderEvent message);
    }
}
