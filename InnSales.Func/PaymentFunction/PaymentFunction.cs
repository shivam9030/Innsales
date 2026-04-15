
// using System.Net;
// using System.Text.Json;
// using Microsoft.Azure.Functions.Worker;
// using Microsoft.Azure.Functions.Worker.Http;
// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.Logging;
// using InnSales.Services;
// using InnSales.Common.DTO;
// using Helper;

// namespace InnSales.Functions
// {
//     public class PaymentFunction
//     {
//         private readonly ILogger<PaymentFunction> _logger;
//         private readonly IConfiguration _config;
//         private readonly IAuthHelper _auth;
//         private readonly IPaymentService _payment;

//         public PaymentFunction(
//             IPaymentService payment,
//             ILogger<PaymentFunction> logger,
//             IConfiguration config,
//             IAuthHelper auth)
//         {
//             _payment = payment;
//             _logger = logger;
//             _config = config;
//             _auth = auth;
//         }

//         [Function("ProcessPayment")]
//         public async Task<HttpResponseData> ProcessPayment(
//             [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/payment/process")] HttpRequestData req)
//         {
//             // Authentication
//             if (!_auth.IsAuthenticated(req))
//                 return _auth.Unauthorized(req);

//             var requestBody = await req.ReadAsStringAsync();
//             Console.WriteLine("Received Payment Request: " + requestBody);

//             var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
//             var request = JsonSerializer.Deserialize<StripePaymentRequestDto>(requestBody, options);

//             if (request == null)
//             {
//                 var badRes = req.CreateResponse(HttpStatusCode.BadRequest);
//                 await badRes.WriteStringAsync("Invalid payment request.");
//                 return badRes;
//             }

//             var response = await _payment.ProcessPaymentAsync(request);

//             var res = req.CreateResponse(response.Success ? HttpStatusCode.OK : HttpStatusCode.BadRequest);
//             await res.WriteAsJsonAsync(response);
//             return res;
//         }
//     }
// }
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using InnSales.Services;
using InnSales.Common.DTO;
using Helper;

namespace InnSales.Functions
{
    public class PaymentFunction
    {
        private readonly ILogger<PaymentFunction> _logger;
        private readonly IConfiguration _config;
        private readonly IAuthHelper _auth;
        private readonly IPaymentService _payment;

        public PaymentFunction(
            IPaymentService payment,
            ILogger<PaymentFunction> logger,
            IConfiguration config,
            IAuthHelper auth)
        {
            _payment = payment;
            _logger = logger;
            _config = config;
            _auth = auth;
        }

        [Function("ProcessPayment")]
        public async Task<HttpResponseData> ProcessPayment(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/payment/process")] HttpRequestData req)
        {
            _logger.LogInformation("Received a payment request");

            // Authentication
            if (!_auth.IsAuthenticated(req))
            {
                _logger.LogWarning("Unauthorized payment request");
                return _auth.Unauthorized(req);
            }

            var requestBody = await req.ReadAsStringAsync();
            _logger.LogInformation("Payment request body: {RequestBody}", requestBody);

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var request = JsonSerializer.Deserialize<StripePaymentRequestDto>(requestBody, options);

            if (request == null)
            {
                _logger.LogWarning("Failed to deserialize payment request");
                var badRes = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRes.WriteStringAsync("Invalid payment request.");
                return badRes;
            }

            _logger.LogInformation("Processing payment for OrderId: {OrderId}, Currency: {Currency}, CardHolder: {CardHolder}", 
                request.OrderId, request.Currency, request.CardHolderName);

            var response = await _payment.ProcessPaymentAsync(request);

            _logger.LogInformation("Payment processing result for OrderId {OrderId}: Success={Success}", 
                request.OrderId, response.Success);

            var res = req.CreateResponse(response.Success ? HttpStatusCode.OK : HttpStatusCode.BadRequest);
            await res.WriteAsJsonAsync(response);
            return res;
        }
    }
}
