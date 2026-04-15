
namespace InnSales.Common.Auth.Webhooks;

public record RegisterWebhookResult(
    string CustomerId,
    string SubscriptionName,
    string Topic,
    string Endpoint
);
