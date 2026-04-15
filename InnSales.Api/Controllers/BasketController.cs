using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InnSales.Services;
using InnSales.Common.DTO;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace InnSales.Api.Controllers
{
    [ApiController]
    [Authorize]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/basket")]
    public class BasketController : ControllerBase
    {
        private readonly IBasketService _basketService;

        public BasketController(IBasketService basketService)
        {
            _basketService = basketService;
        }

        [HttpGet]
        public async Task<IActionResult> GetBasket()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var items = await _basketService.GetBasketAsync(userId);
            return Ok(items);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToBasket([FromBody] AddBasketItemDto dto)
        {
            dto.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(dto.UserId)) return Unauthorized();

            await _basketService.AddToBasketAsync(dto);
            return Ok(new { message = "Item added to basket." });
        }

      
       [HttpPut("update/{id}")]
public async Task<IActionResult> UpdateQuantity(Guid id, [FromBody] int quantity)
{
    try
    {
        await _basketService.UpdateQuantityAsync(id, quantity);
        return Ok(new { success = true, updatedQuantity = quantity });
    }
    catch (Exception ex)
    {
        return BadRequest(new { error = ex.Message });
    }
}

        [HttpDelete("remove/{id}")]
        public async Task<IActionResult> RemoveItem(Guid id)
        {
            await _basketService.RemoveItemAsync(id);
            return Ok(new { message = "Item removed from basket." });
        }

     
[HttpPost("checkout")]
public async Task<IActionResult> Checkout()
{
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(userId)) return Unauthorized();

    try
    {
        var order = await _basketService.CheckoutAsync(userId);
        return Ok(order);
    }
    catch (Exception ex)
    {
        // Log error (use ILogger)
        return BadRequest(new { message = "Checkout failed, please try again.", error = ex.Message });
    }
}

        [HttpPost("apply-promo")]
public async Task<IActionResult> ApplyPromoCode([FromBody] string promoCode)
{
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(userId)) return Unauthorized();

    try
    {
        await _basketService.ApplyPromoCodeAsync(userId, promoCode);
        return Ok(new { message = "Promo code applied successfully." });
    }
    catch (Exception ex)
    {
        return BadRequest(new { error = ex.Message });
    }
}
    }
}