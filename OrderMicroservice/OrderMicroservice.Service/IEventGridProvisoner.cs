
using System;
using System.Threading.Tasks;

namespace OrderMicroservice.Service
{
    public interface IEventGridProvisioner
    {
        Task EnsureTopicAsync(string topicName);

        Task CreateVendorSubscriptionAsync(
            string topic,
            string subscriptionName,
            Uri endpoint,
            string filterField,
            string filterValue);
    }
}
