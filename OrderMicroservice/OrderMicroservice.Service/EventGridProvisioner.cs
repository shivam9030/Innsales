
using System;
using System.Threading.Tasks;
using MockEventGrid;
namespace OrderMicroservice.Service
{
    public sealed class MockEventGridProvisioner : IEventGridProvisioner
    {
        private readonly MockEventGridBroker _broker;

        public MockEventGridProvisioner(MockEventGridBroker broker)
        {
            _broker = broker;
        }

        public Task EnsureTopicAsync(string topicName)
        {
            _broker.CreateTopic(topicName);
            return Task.CompletedTask;
        }

        public Task CreateVendorSubscriptionAsync(
            string topic,
            string subscriptionName,
            Uri endpoint,
            string filterField,
            string filterValue)
        {
            _broker.CreateSubscription(
                topic,
                new EventSubscriptionDefinition(
                    Name: subscriptionName,
                    Endpoint: endpoint,
                    Filters: new[]
                    {
                        new AdvancedFilter(
                            filterField,
                            "StringEquals",
                            new[] { filterValue }
                        )
                    }
                )
            );

            Console.WriteLine($"[MockEventGrid] Subscription '{subscriptionName}' => {endpoint} for {filterField}={filterValue}");
            return Task.CompletedTask;
        }
    }
}
