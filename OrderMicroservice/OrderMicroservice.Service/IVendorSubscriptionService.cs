
using System;
using System.Threading.Tasks;

namespace OrderMicroservice.Service
{
    public interface IVendorSubscriptionService
    {
        Task<RegisterVendorSubscriptionResult> RegisterAsync(Guid vendorId, Uri webhookEndpoint);
    }

    public sealed record RegisterVendorSubscriptionResult(
        Guid VendorId,
        string Topic,
        string SubscriptionName,
        string Endpoint
    );
}

