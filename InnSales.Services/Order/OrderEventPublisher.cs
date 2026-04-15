using MockEventGrid;
using System.Text.Json;

public class OrderEventPublisher : IOrderEventPublisher
{
    private readonly MockEventGridBroker _broker;

    public OrderEventPublisher(MockEventGridBroker broker)
    {
        _broker = broker;
    }

    public async Task PublishOrderUpdatedAsync(
        Guid orderId,
        string customerId,
        string status)
    {
        var ev = new MockEventGridEvent(
            Subject: $"Order/{orderId}",
            EventType: "Order.Updated",
            DataVersion: "1.0",
            Data: JsonSerializer.SerializeToElement(new
            {
                OrderId = orderId,
                CustomerId = customerId,
                Status = status,
                UpdatedAt = DateTime.UtcNow
            }),
            EventTime: DateTimeOffset.UtcNow
        );

        Console.WriteLine("Publishing event to MockEventGridBroker..."+$"{orderId}+{customerId}+{status}");

        await _broker.PublishAsync("order-updates-topic", new[] { ev });
    }
}
