
using Microsoft.AspNetCore.Mvc;
using InnSales.Common.Auth.Webhooks;
using Microsoft.AspNetCore.Authorization;
using InnSales.Services.Authentication.Webhooks;
using System.Security.Claims;
namespace InnSales.API.Controllers
{

[ApiController]
[Route("api/auth/webhooks")]
public sealed class AuthWebhooksController : ControllerBase
{
    private readonly IAuthWebhookRegistrationService _service;

    public AuthWebhooksController(IAuthWebhookRegistrationService service)
    {
        _service = service;
    }

    [HttpPost("register")]
    [Authorize]
    public async Task<ActionResult<RegisterWebhookResult>> Register([FromBody] RegisterWebhookBody body)
    {
        if (string.IsNullOrWhiteSpace(body?.WebhookUrl))
            return BadRequest("WebhookUrl is required.");

        // Prefer a dedicated claim like "customer_id"
        var customerId =
            User.FindFirst("customer_id")?.Value ??
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
            User.FindFirst("sub")?.Value;

        if (string.IsNullOrWhiteSpace(customerId))
            return Unauthorized("CustomerId not found in token.");

        // Compose the service request with the claim-derived customerId
        var request = new RegisterWebhookRequest(
            CustomerId: customerId,
            WebhookUrl: body.WebhookUrl,
            Secret: body.Secret
        );

        var result = await _service.RegisterClientWebhookAsync(request);
        return Ok(result);
    }
public record RegisterWebhookBody(string WebhookUrl, string? Secret);
}
}