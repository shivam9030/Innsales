using Microsoft.AspNetCore.Mvc;
using InnSales.Services;
using InnSales.Common.DTO;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/promocodes")]
public class PromoCodeController : ControllerBase
{
    private readonly IPromoCodeService _promoCodeService;

    public PromoCodeController(IPromoCodeService promoCodeService)
    {
        _promoCodeService = promoCodeService;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreatePromoCode([FromBody] PromoCodeCreateDto dto)
    {
        var result = await _promoCodeService.CreatePromoCodeAsync(dto);
        return Ok(result);
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdatePromoCode([FromBody] PromoCodeUpdateDto dto)
    {
        var result = await _promoCodeService.UpdatePromoCodeAsync(dto);
        return Ok(result);
    }

    [HttpDelete("{promoCodeId}")]
    public async Task<IActionResult> DeletePromoCode(Guid promoCodeId)
    {
        var success = await _promoCodeService.DeletePromoCodeAsync(promoCodeId);
        return success ? Ok(new { message = "Promo code deleted" }) : NotFound();
    }

    [HttpGet("{promoCodeId}")]
    public async Task<IActionResult> GetPromoCodeById(Guid promoCodeId)
    {
        var result = await _promoCodeService.GetPromoCodeByIdAsync(promoCodeId);
        return result != null ? Ok(result) : NotFound();
    }

    [HttpGet("promotion/{promotionId}")]
    public async Task<IActionResult> GetPromoCodesByPromotion(Guid promotionId)
    {
        var result = await _promoCodeService.GetPromoCodesByPromotionAsync(promotionId);
        return Ok(result);
    }
}