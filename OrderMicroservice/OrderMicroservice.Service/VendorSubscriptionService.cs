
using System;
using System.Threading.Tasks;

namespace OrderMicroservice.Service
{
    public sealed class VendorSubscriptionService : IVendorSubscriptionService
    {
        private readonly IEventGridProvisioner _provisioner;
        private const string Topic = "order-updates-topic";

        public VendorSubscriptionService(IEventGridProvisioner provisioner)
        {
            _provisioner = provisioner;
        }

        public async Task<RegisterVendorSubscriptionResult> RegisterAsync(Guid vendorId, Uri webhookEndpoint)
        {
            await _provisioner.EnsureTopicAsync(Topic);

            // Simple subscription name
            var subscriptionName = $"vendor-{vendorId}";

            await _provisioner.CreateVendorSubscriptionAsync(
                topic: Topic,
                subscriptionName: subscriptionName,
                endpoint: webhookEndpoint,
                filterField: "data.vendorId",
                filterValue: vendorId.ToString()
            );

            return new RegisterVendorSubscriptionResult(
                VendorId: vendorId,
                Topic: Topic,
                SubscriptionName: subscriptionName,
                Endpoint: webhookEndpoint.ToString()
            );
        }
    }
}
