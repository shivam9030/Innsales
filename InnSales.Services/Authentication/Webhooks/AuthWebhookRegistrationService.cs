
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using InnSales.Common.Auth.Webhooks;
using InnSales.Services.Authentication.EventGrid;
using InnSales.Domain;
using InnSales.DataBase;

namespace InnSales.Services.Authentication.Webhooks
{
    public sealed class AuthWebhookRegistrationService : IAuthWebhookRegistrationService
    {
        private readonly InnSalesDbContext  _db;
        private readonly IAuthEventGridProvisioningService _provisioner;

        private const string Topic = "order-updates-topic";

        public AuthWebhookRegistrationService(InnSalesDbContext db, IAuthEventGridProvisioningService provisioner)
        {
            _db = db;
            _provisioner = provisioner;
        }

    public async Task<RegisterWebhookResult> RegisterClientWebhookAsync(RegisterWebhookRequest request)
        {
            var client = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.CustomerId)
                         ?? throw new InvalidOperationException($"Client '{request.CustomerId}' not found.");

            if (string.IsNullOrWhiteSpace(request.WebhookUrl))
                throw new ArgumentException("WebhookUrl is required.", nameof(request.WebhookUrl));

            client.WebhookUrl = request.WebhookUrl.Trim();
            client.WebhookSecret = request.Secret;
            client.SubscriptionName ??= $"client{client.Id}-sub";

            await _db.SaveChangesAsync();

            await _provisioner.EnsureTopicAsync(Topic);

            var filterValue = (object)client.Id;

            await _provisioner.CreateClientSubscriptionAsync(
                Topic,
                client.SubscriptionName!,
                               new Uri(client.WebhookUrl!),
                filterValue
            );

            return new RegisterWebhookResult(
                client.Id,
                client.SubscriptionName!,
                Topic,
                client.WebhookUrl!
            );
        }
    }
}
