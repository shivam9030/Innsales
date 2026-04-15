
namespace InnSales.Common.Auth.Webhooks;

public record RegisterWebhookRequest(
    string CustomerId,
    string WebhookUrl,
    string? Secret
);
