using InnSales.Common.DTO;

namespace InnSales.Services
{
    public interface IPromoCodeService
    {
        Task<PromoCodeResponseDto> CreatePromoCodeAsync(PromoCodeCreateDto dto);
        Task<PromoCodeResponseDto> UpdatePromoCodeAsync(PromoCodeUpdateDto dto);
        Task<bool> DeletePromoCodeAsync(Guid promoCodeId);
        Task<PromoCodeResponseDto> GetPromoCodeByIdAsync(Guid promoCodeId);
        Task<IEnumerable<PromoCodeResponseDto>> GetPromoCodesByPromotionAsync(Guid promotionId);

        /// Apply promo code to basket items
        Task<bool> ApplyPromoCodeAsync(string userId, string code);

        /// Calculate discount based on promo code and subtotal
        Task<decimal> CalculateDiscountAsync(string promoCode, decimal subtotal);

      decimal CalculateEffectivePrice(decimal originalPrice, Guid? promoCodeId);

    }
}