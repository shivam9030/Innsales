using Microsoft.AspNetCore.Mvc;
using InnSales.Services;
using InnSales.Common.DTO;
using Microsoft.AspNetCore.Authorization;
using InnSales.Common.Enums;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/promotions")]
public class PromotionController : ControllerBase
{
    private readonly IPromotionService _promotionService;

    public PromotionController(IPromotionService promotionService)
    {
        _promotionService = promotionService;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreatePromotion([FromBody] PromotionCreateDto dto)
    {
        var result = await _promotionService.CreatePromotionAsync(dto);
        return Ok(result);
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdatePromotion([FromBody] PromotionUpdateDto dto)
    {
        var result = await _promotionService.UpdatePromotionAsync(dto);
        return Ok(result);
    }

    [HttpDelete("{promotionId}")]
    public async Task<IActionResult> DeletePromotion(Guid promotionId)
    {
        var success = await _promotionService.DeletePromotionAsync(promotionId);
        return success ? Ok(new { message = "Promotion deleted" }) : NotFound();
    }

    [HttpGet("{promotionId}")]
    public async Task<IActionResult> GetPromotionById(Guid promotionId)
    {
        var result = await _promotionService.GetPromotionByIdAsync(promotionId);
        return result != null ? Ok(result) : NotFound();
    }

    [HttpGet]
    public async Task<IActionResult> GetAllPromotions()
    {
        var result = await _promotionService.GetAllPromotionsAsync();
        return Ok(result);
    }

    [HttpPatch("{promotionId}/status")]
    public async Task<IActionResult> ChangeStatus(Guid promotionId, [FromQuery]  PromotionStatus status)
    {
        var success = await _promotionService.ChangePromotionStatusAsync(promotionId, status);
        return success ? Ok(new { message = "Status updated" }) : NotFound();
    }
}