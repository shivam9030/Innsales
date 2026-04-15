using System;
using MockEventGrid;
namespace OrderMicroservice.Service
{   
public sealed class VendorOrderSubscriptionService
{
    private readonly MockEventGridBroker _broker;

    public VendorOrderSubscriptionService(MockEventGridBroker broker)
    {
        _broker = broker;
    }

  public void RegisterSubscriptions(Guid vendorId)
{
    const string topic = "order-updates-topic";

    _broker.CreateTopic(topic);

    _broker.CreateSubscription(
        topic,
        new EventSubscriptionDefinition(
            Name: $"vendor-{vendorId}",
            Endpoint: new Uri("http://localhost:5000/api/v1/order-events"),
            Filters: new[]
            {
                new AdvancedFilter(
                "data.vendorId",
                    "StringEquals", new[] { vendorId.ToString() }
                )
            }
        )
    );

    Console.WriteLine($"[MockEventGrid] Subscription registered for Vendor {vendorId}");
}

}
}