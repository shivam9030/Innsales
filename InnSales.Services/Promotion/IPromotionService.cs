using InnSales.Common.Enums;
using InnSales.Common.DTO;

namespace InnSales.Services
{
    public interface IPromotionService
    {
        // Create a new promotion.
        Task<PromotionResponseDto> CreatePromotionAsync(PromotionCreateDto dto);
        // Update an existing promotion.
        Task<PromotionResponseDto> UpdatePromotionAsync(PromotionUpdateDto dto);
        // Delete a promotion by its ID.
        Task<bool> DeletePromotionAsync(Guid promotionId);
        // Get promotion details by ID.
        Task<PromotionResponseDto> GetPromotionByIdAsync(Guid promotionId);
        // Get all promotions.
        Task<IEnumerable<PromotionResponseDto>> GetAllPromotionsAsync();
        // Change promotion status (e.g., Activate, Deactivate).
        Task<bool> ChangePromotionStatusAsync(Guid promotionId, PromotionStatus status);
    }
}