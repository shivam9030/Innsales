using System.Threading.Tasks;
using OrderMicroservice.Common.DTO;

namespace OrderMicroservice.Service
{
    public interface IOrderEventPublisher
    {
        Task PublishAsync(OrderEvent message);
    }
}
