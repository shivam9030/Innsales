using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InnSales.Services;
using InnSales.Common.DTO;
using System.Security.Claims;
namespace InnSales.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/orders")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IBasketService _basketService;

        public OrderController(IOrderService orderService, IBasketService basketService)
        {
            _orderService = orderService;
            _basketService = basketService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDto>> GetOrderById(Guid id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            return order == null ? NotFound() : Ok(order);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetAllOrders()
        {  var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var orders = await _orderService.GetAllOrdersAsync(userId);
            return Ok(orders);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(Guid id)
        {
            try
            {
                await _orderService.DeleteOrderAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Order not found.");
            }
        }

        [Authorize]
        [HttpPost("create")]
        public async Task<ActionResult<OrderDto>> CreateOrderFromBasket([FromQuery] string userId)
        {
            try
            {
                var basket = await _basketService.GetBasketAsync(userId);
                // Controller no longer checks basket; service handles empty basket
                var order = await _orderService.CreateOrderFromBasketAsync(userId, basket);
                return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, order);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost("cancel/{orderId}")]
        public async Task<IActionResult> CancelOrder(Guid orderId)
        {
            try
            {
                await _orderService.CancelOrderAsync(orderId);
                return Ok("Order cancelled and inventory restocked.");
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Order not found.");
            }
        }

        // [Authorize]
        // [HttpPost("handle-failed-payment/{orderId}")]
        // public async Task<IActionResult> HandleFailedPayment(Guid orderId)
        // {
        //     try
        //     {
        //         await _orderService.HandleFailedPaymentAsync(orderId);
        //         return Ok("Inventory restocked due to failed payment.");
        //     }
        //     catch (KeyNotFoundException)
        //     {
        //         return NotFound("Order not found.");
        //     }
        // }
    }
}
