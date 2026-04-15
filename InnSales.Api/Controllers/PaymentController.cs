using Microsoft.AspNetCore.Mvc;
using InnSales.Services;
using InnSales.Common.DTO;
using System.Threading.Tasks;

namespace InnSales.Api.Controllers
{  
    [ApiController]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/payment")]
    public class StripePaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public StripePaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost("process")]
        public async Task<IActionResult> ProcessPayment([FromBody] StripePaymentRequestDto request)
        {
            // Moved null/empty token check to service
            var response = await _paymentService.ProcessPaymentAsync(request);

            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }
    }
}
