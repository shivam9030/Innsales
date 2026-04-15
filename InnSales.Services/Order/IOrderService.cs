
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InnSales.Common.DTO;

namespace InnSales.Services
{
    public interface IOrderService
    {
        Task<OrderDto> CreateOrderFromBasketAsync(string userId, BasketResponseDto basket);
        Task<OrderDto> GetOrderByIdAsync(Guid id);
        Task<IEnumerable<OrderDto>> GetAllOrdersAsync(string userId);
        Task CancelOrderAsync(Guid orderId);
        Task HandleFailedPaymentAsync(Guid orderId);
        Task DeleteOrderAsync(Guid id);
        Task HandlePaymentTransactionAsync(PaymentTransactionMessage message);
    }
}
