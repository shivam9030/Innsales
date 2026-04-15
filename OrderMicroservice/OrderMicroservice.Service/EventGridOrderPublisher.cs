
using System;
using System.Text.Json;
using System.Threading.Tasks;
using MockEventGrid;
using OrderMicroservice.Common.DTO;

namespace OrderMicroservice.Service
{
    public class EventGridOrderPublisher : IOrderEventPublisher
    {
        private readonly MockEventGridBroker _broker;
        private const string TopicName = "order-updates-topic";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        public EventGridOrderPublisher(MockEventGridBroker broker)
        {
            _broker = broker ?? throw new ArgumentNullException(nameof(broker));
        }

        public async Task PublishAsync(OrderEvent message)
        {
            if (message is null) throw new ArgumentNullException(nameof(message));
            if (message.VendorId == Guid.Empty) throw new ArgumentException("VendorId cannot be empty.", nameof(message));
            if (message.OrderId == Guid.Empty) throw new ArgumentException("OrderId cannot be empty.", nameof(message));

            // Ensure topic exists (idempotent in mock)
            _broker.CreateTopic(TopicName);

            // Build event payload
            var dataElement = JsonSerializer.SerializeToElement(new
            {
                vendorId = message.VendorId.ToString(),
                orderId = message.OrderId,
                customerId = message.CustomerId,
                orderStatus = message.OrderStatus,
                paymentStatus = message.PaymentStatus,
                createdAt = DateTime.UtcNow
            }, JsonOptions);

            var ev = new MockEventGridEvent(
                Subject: $"vendor/{message.VendorId}/orders/{message.OrderId}",
                EventType: "SalesOrderCreated",
                DataVersion: "1.0",
                Data: dataElement,
                EventTime: DateTimeOffset.UtcNow
            );

            await _broker.PublishAsync(TopicName, new[] { ev });

            Console.WriteLine($"[Publisher] Event published for OrderId={message.OrderId}, VendorId={message.VendorId}");
        }
    }
}
