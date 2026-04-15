using System;
using System.Threading.Tasks;
using InnSales.Common.Auth.Webhooks;
using InnSales.Services.Authentication.EventGrid;
using MockEventGrid;
namespace InnSales.Services.Authentication.EventGrid
{
    public class AuthEventGridProvisioningService:IAuthEventGridProvisioningService
    {
        private readonly MockEventGridBroker _broker;
        public AuthEventGridProvisioningService(MockEventGridBroker broker)
        {
            _broker = broker;
        }
        public Task EnsureTopicAsync(string topicName)
        {

            _broker.CreateTopic(topicName);
            return Task.CompletedTask;
        }

        public Task CreateClientSubscriptionAsync(string topicName,string subscriptionName, Uri endpoint, object customerFilterValue, int maxDeliveryAttempts = 6, TimeSpan? baseRetryDelay = null, string deadLetterContainer = "deadletter")
        {
            _broker.CreateSubscription(
                topicName,
                new EventSubscriptionDefinition(
                    Name: subscriptionName,
                    Endpoint: endpoint,
                    Filters: new[]
                    {
                        new AdvancedFilter("data.CustomerId", "StringEquals", new[] { customerFilterValue.ToString() })
                    }
                )
            );
            return Task.CompletedTask;
        }

    }
}