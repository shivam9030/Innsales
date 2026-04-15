using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace InnSales.Controllers
{
    [ApiController]
    
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/order-updates")]

    public class AuthEventsController : ControllerBase
    {[HttpPost]
public IActionResult Receive([FromBody] JsonElement events)
{
    Console.WriteLine("Auth service received EventGrid event:");
    Console.WriteLine(events.ToString());
      
    return Ok();
}

    }
}
