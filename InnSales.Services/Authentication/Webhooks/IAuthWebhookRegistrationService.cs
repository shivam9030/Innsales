
using System.Threading.Tasks;
using InnSales.Common.Auth.Webhooks;

namespace InnSales.Services.Authentication.Webhooks
{
    public interface IAuthWebhookRegistrationService
    {
        Task<RegisterWebhookResult> RegisterClientWebhookAsync(RegisterWebhookRequest request);
    }
}
