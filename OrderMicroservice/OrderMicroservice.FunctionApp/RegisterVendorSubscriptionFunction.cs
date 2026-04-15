
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OrderMicroservice.Service;

namespace OrderMicroservice.FunctionApp
{
    public sealed class RegisterVendorSubscriptionFunction
    {
        private readonly IVendorSubscriptionService _service;

        public RegisterVendorSubscriptionFunction(IVendorSubscriptionService service)
        {
            _service = service;
        }

        [Function("RegisterVendorSubscription")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "vendors/subscriptions")]
            HttpRequestData req,
            FunctionContext ctx)
        {
            var log = ctx.GetLogger("RegisterVendorSubscription");

            var bodyText = await new StreamReader(req.Body).ReadToEndAsync();
            var body = JsonSerializer.Deserialize<RequestBody>(bodyText, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (body is null || string.IsNullOrWhiteSpace(body.WebhookUrl) || string.IsNullOrWhiteSpace(body.VendorId))
            {
                var bad = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await bad.WriteStringAsync("WebhookUrl and VendorId are required.");
                return bad;
            }

            if (!Guid.TryParse(body.VendorId, out var vendorGuid))
            {
                var bad = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await bad.WriteStringAsync("VendorId must be a valid GUID.");
                return bad;
            }

            Uri endpoint;
            try { endpoint = new Uri(body.WebhookUrl, UriKind.Absolute); }
            catch
            {
                var bad = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await bad.WriteStringAsync("WebhookUrl must be an absolute URL.");
                return bad;
            }

            var result = await _service.RegisterAsync(vendorGuid, endpoint);

            var ok = req.CreateResponse(System.Net.HttpStatusCode.OK);
            await ok.WriteAsJsonAsync(result);
            log.LogInformation("Subscription created: {Name} -> {Endpoint}", result.SubscriptionName, result.Endpoint);
            return ok;
        }

        private sealed class RequestBody
        {
            public string? WebhookUrl { get; set; }
            public string? VendorId { get; set; }
        }
    }
}
