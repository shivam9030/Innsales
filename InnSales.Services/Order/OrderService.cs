using InnSales.DataBase;
using InnSales.Common.DTO;
using InnSales.Domain.Entities;
using InnSales.Common.Enums;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using MockEventGrid;
using System.Threading.Tasks;
using System.Text.Json;
using System.Net.Http;
using System.Text;

namespace InnSales.Services
{
    public class OrderService : IOrderService
    {
        private readonly InnSalesDbContext _context;
        private readonly IMapper _mapper;
        private readonly IInventoryService _inventoryService;
        private readonly IOrderEventPublisher _eventPublisher;

        private readonly IPaymentTokenService _paymentTokenService;

        public OrderService(
            InnSalesDbContext context,
            IMapper mapper,
            IInventoryService inventoryService,
            IOrderEventPublisher eventPublisher, IPaymentTokenService paymentTokenService)
        {
            _context = context;
            _mapper = mapper;
            _inventoryService = inventoryService;
            _eventPublisher = eventPublisher;
            _paymentTokenService = paymentTokenService;
        }


        public async Task<OrderDto> CreateOrderFromBasketAsync(string userId, BasketResponseDto basketSummary)
        {
            // Move empty basket check from controller to service
            if (basketSummary == null || !basketSummary.Items.Any())
                throw new InvalidOperationException("Basket is empty.");

            var basketItems = await _context.BasketItems
                .Include(b => b.Product)
                .Where(b => b.UserId == userId)
                .ToListAsync();

            var order = await BuildOrderAsync(userId, basketSummary, basketItems);

            _context.Orders.Add(order);
            _context.BasketItems.RemoveRange(basketItems);
            await _context.SaveChangesAsync();
            Console.WriteLine("Publishing Order Created Event..." + order.Id + "," + order.CustomerId);

            // await _eventPublisher.PublishOrderUpdatedAsync(
            //     order.Id,
            //     order.CustomerId,
            //     OrderStatus.Pending.ToString());

    //  await SendPublishPayloadAsync(order.Id, order.CustomerId, order.OrderStatus.ToString(), order.PaymentStatus.ToString());

            var dto = _mapper.Map<OrderDto>(order);
            dto.Subtotal = basketSummary.TotalPrice;
            dto.Discount = basketSummary.DiscountAmount;
            dto.TotalAmount = basketSummary.DiscountedTotal;
            return dto;
        }

        public async Task<OrderDto> GetOrderByIdAsync(Guid id)
        {
            var order = await GetOrderEntityAsync(id);
            var dto = _mapper.Map<OrderDto>(order);
            CalculateOrderTotals(order, dto);
            if (order.PaymentStatus == PaymentStatus.Pending)
            {
                dto.PaymentToken = _paymentTokenService.Issue(
                    order.Id,
                    dto.TotalAmount,
                    "inr",
                    order.CustomerId
                );
    }

    return dto;
        }

        public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync(string userId)
        {
            var orders = await _context.Orders
            .Where(o=> o.CustomerId == userId)
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .ToListAsync();

            var dtos = _mapper.Map<IEnumerable<OrderDto>>(orders);
            foreach (var (order, dto) in orders.Zip(dtos))
            {
                CalculateOrderTotals(order, dto);
            }
            return dtos;
        }

        public async Task CancelOrderAsync(Guid orderId)
        {
            var order = await GetOrderEntityAsync(orderId);
            order.OrderStatus = OrderStatus.Cancelled;
            await _context.SaveChangesAsync();
            await _eventPublisher.PublishOrderUpdatedAsync(
                order.Id,
                order.CustomerId,
                OrderStatus.Cancelled.ToString());

            await _inventoryService.RestockInventoryAsync(orderId);
        }

        public async Task HandleFailedPaymentAsync(Guid orderId)
        {
            var order = await GetOrderEntityAsync(orderId);
            if (order.PaymentStatus != PaymentStatus.Success)
            {
                await _inventoryService.RestockInventoryAsync(orderId);
            }
        }
        public async Task HandlePaymentTransactionAsync(PaymentTransactionMessage msg)
        {
             if (!msg.Success && msg.TransactionId == null)
    {
        Console.WriteLine(
            $"[OrderService] Ignoring non-payment event. OrderId={msg.OrderId}"
        );
        return;
    }
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == msg.OrderId);

            if (order == null)
                throw new Exception("Order not found");

            if (msg.Success)
            {
                order.PaymentStatus = PaymentStatus.Success;
                order.OrderStatus = OrderStatus.Confirmed;

                order.TransactionId = msg.TransactionId;
                order.CardHolderName = msg.CardHolderName;
                order.MaskedCardNumber = msg.MaskedCardNumber;
                order.CardExpiryDate = msg.CardExpiryDate;
            }
            else
            {
                order.PaymentStatus = PaymentStatus.Failed;
                order.OrderStatus = OrderStatus.Cancelled;
                await _inventoryService.RestockInventoryAsync(order.Id);
            }
            //Update or Create Payment record
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.OrderId == order.Id);

            if (payment == null)
            {
                payment = new Payment
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    ReadableOrderId = order.ReadableOrderId,
                    TransactionId = msg.TransactionId,
                    CardHolderName = msg.CardHolderName,
                    MaskedCardNumber = msg.MaskedCardNumber,
                    CardExpiryDate = msg.CardExpiryDate,
                    PaymentStatus = msg.Success ? PaymentStatus.Success : PaymentStatus.Failed,
                    AmountPaid = order.TotalAmount,
                    PaymentDate = msg.OccurredAtUtc,
                    TransactionTime = TimeSpan.Zero
                };
                _context.Payments.Add(payment);
            }
            else
            {
                payment.TransactionId = msg.TransactionId;
                payment.CardHolderName = msg.CardHolderName;
                payment.MaskedCardNumber = msg.MaskedCardNumber;
                payment.CardExpiryDate = msg.CardExpiryDate;
                payment.PaymentStatus = msg.Success ? PaymentStatus.Success : PaymentStatus.Failed;
                payment.AmountPaid = order.TotalAmount;
                payment.PaymentDate = msg.OccurredAtUtc;
            }

            // If payment failed, attempt to record a FailedTransaction 
            if (!msg.Success)
            {
                try
                {
                    var failed = new FailedTransaction
                    {
                        Id = Guid.NewGuid(),
                        OrderId = order.Id,
                        TransactionId = msg.TransactionId ?? string.Empty,
                        FailureMessage = msg.FailureReason ?? "Payment failed",
                        Amount = order.TotalAmount,
                        Currency = "unknown",
                        Description = string.Empty,
                        CardHolderName = msg.CardHolderName ?? string.Empty,
                        LoggedDate = msg.OccurredAtUtc
                    };
                    _context.FailedTransactions.Add(failed);
                }
                catch (Exception ex)
                {

                    Console.WriteLine($"[OrderService] Failed to record FailedTransaction for Order {order.Id}: {ex.Message}\n{ex.StackTrace}");
                }
            }

            await _context.SaveChangesAsync();
            await _eventPublisher.PublishOrderUpdatedAsync(
            order.Id,
            order.CustomerId,
            order.OrderStatus.ToString());

        }

        public async Task DeleteOrderAsync(Guid id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                throw new KeyNotFoundException("Order not found");

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
        }

        private async Task<Order> BuildOrderAsync(string userId, BasketResponseDto basketSummary, List<BasketItem> basketItems)
        {
            var orderId = Guid.NewGuid();
            var readableOrderId = await GenerateReadableOrderIdAsync();

            var order = new Order
            {
                Id = orderId,
                ReadableOrderId = readableOrderId,
                CustomerId = userId,
                OrderDate = DateTime.UtcNow,
                RequiredByDate = DateTime.UtcNow.AddDays(3),
                OrderStatus = OrderStatus.Pending,
                PaymentStatus = PaymentStatus.Pending,
                OrderItems = new List<OrderItem>(),
                Tax = basketSummary.TotalPrice * 0.18m,
                TotalAmount = basketSummary.DiscountedTotal + (basketSummary.TotalPrice * 0.18m)
            };

            foreach (var item in basketItems)
            {
                var unitPrice = item.Product.Price;
                if (item.Product.IsPromoProduct && basketSummary.Items.Any(b => b.ProductId == item.ProductId))
                {
                    var basketItemDto = basketSummary.Items.First(b => b.ProductId == item.ProductId);
                    unitPrice = basketItemDto.EffectivePrice;
                }

                order.OrderItems.Add(new OrderItem
                {
                    OrderId = orderId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = unitPrice
                });
            }

            return order;
        }

        private async Task<Order> GetOrderEntityAsync(Guid orderId)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId)
                ?? throw new KeyNotFoundException("Order not found");
        }

        private void CalculateOrderTotals(Order order, OrderDto dto)
        {
            decimal subtotal = order.OrderItems.Sum(oi => oi.Quantity * oi.UnitPrice);
            decimal discount = subtotal - (order.TotalAmount - order.Tax);

            dto.Subtotal = subtotal;
            dto.Discount = discount;
            dto.Tax = order.Tax;
            dto.TotalAmount = subtotal - discount + order.Tax;
        }

        private async Task<string> GenerateReadableOrderIdAsync()
        {
            var today = DateTime.UtcNow.Date;
            var todayOrdersCount = await _context.Orders
                .CountAsync(o => o.OrderDate.Date == today);

            return $"ORD-{today:yyyyMMdd}-{(todayOrdersCount + 1):D5}";
        }

    public async Task SendPublishPayloadAsync(Guid orderId, string customerId, string orderStatus, string paymentStatus)
{
    var payload = new
    {
        vendorId = "ee11f1f1-7854-4b01-90a6-d701748f0877",
        orderId = orderId,
        customerId = customerId,
        orderStatus =   orderStatus,
        paymentStatus = paymentStatus
    };

    var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    });

    using var http = new HttpClient();
    var content = new StringContent(json, Encoding.UTF8, "application/json");

    var response = await http.PostAsync("http://localhost:7071/api/v1/order-message/publish", content);

    if (!response.IsSuccessStatusCode)
    {
        var error = await response.Content.ReadAsStringAsync();
        throw new Exception($"Publish failed: {response.StatusCode} - {error}");
    }
}

    }
    
}
