using Microsoft.EntityFrameworkCore;
using InnSales.DataBase;
using InnSales.Domain.Entities;
using InnSales.Common.DTO;
using InnSales.Common.Enums;

namespace InnSales.Services
{
    public class PromoCodeService : IPromoCodeService
    {
        private readonly InnSalesDbContext _context;
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;

        public PromoCodeService(InnSalesDbContext context, ICategoryService categoryService, IProductService productService)
        {
            _context = context;
            _categoryService = categoryService;
            _productService = productService;
        }

        public async Task<PromoCodeResponseDto> CreatePromoCodeAsync(PromoCodeCreateDto dto)
        {
            var promoCode = new PromoCode
            {
                PromoCodeId = Guid.NewGuid(),
                PromotionId = dto.PromotionId,
                Code = dto.Code,
                isUniqueCode = dto.IsUniqueCode,
                MaxUsageLimit = dto.MaxUsageLimit,
                CreatedAt = DateTime.UtcNow
            };

            _context.PromoCodes.Add(promoCode);
            await _context.SaveChangesAsync();

            return MapToResponseDto(promoCode);
        }

        public async Task<PromoCodeResponseDto> UpdatePromoCodeAsync(PromoCodeUpdateDto dto)
        {
            var promoCode = await _context.PromoCodes.FindAsync(dto.PromoCodeId)
                ?? throw new Exception("Promo code not found");

            promoCode.MaxUsageLimit = dto.MaxUsageLimit;
            promoCode.isUniqueCode = dto.IsUniqueCode;

            await _context.SaveChangesAsync();

            return MapToResponseDto(promoCode);
        }

        public async Task<bool> DeletePromoCodeAsync(Guid promoCodeId)
        {
            var promoCode = await _context.PromoCodes.FindAsync(promoCodeId);
            if (promoCode == null) return false;

            _context.PromoCodes.Remove(promoCode);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<PromoCodeResponseDto> GetPromoCodeByIdAsync(Guid promoCodeId)
        {
            var promoCode = await _context.PromoCodes.FindAsync(promoCodeId);
            return promoCode == null ? null : MapToResponseDto(promoCode);
        }

        public async Task<IEnumerable<PromoCodeResponseDto>> GetPromoCodesByPromotionAsync(Guid promotionId)
        {
            var promoCodes = await _context.PromoCodes
                .Where(pc => pc.PromotionId == promotionId)
                .ToListAsync();

            return promoCodes.Select(MapToResponseDto);
        }

   

        public async Task<bool> ApplyPromoCodeAsync(string userId, string code)
        {
            var promoCode = await _context.PromoCodes
                .Include(pc => pc.Promotion)
                .FirstOrDefaultAsync(pc => pc.Code == code);

            var basketItems = await _context.BasketItems
                .Include(b => b.Product)
                .Where(b => b.UserId == userId)
                .ToListAsync();

            if (!basketItems.Any()) throw new Exception("Basket is empty");

            ValidatePromoCode(promoCode, basketItems);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                ApplyPromoToBasketItems(basketItems, promoCode);

                var promoCategoryId = await _categoryService.EnsurePromoCategoryAsync();
                var promoProductId = await _productService.EnsurePromoProductAsync(promoCategoryId);

                await AddPromoItemIfMissing(userId, promoCode, promoProductId);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"Failed to apply promo code: {ex.Message}", ex);
            }
        }

        private void ValidatePromoCode(PromoCode promoCode, IEnumerable<BasketItem> basketItems)
        {
            if (promoCode == null) throw new Exception("Invalid promo code");

            var promotion = promoCode.Promotion;

            if (promotion.Status != PromotionStatus.Active ||
                DateTime.UtcNow < promotion.StartDate ||
                DateTime.UtcNow > promotion.EndDate)
                throw new Exception("Promo code expired or inactive");

            if (promoCode.isUniqueCode && promoCode.UsageCount > 0)
                throw new Exception("Promo code already used");

            if (promoCode.UsageCount >= promoCode.MaxUsageLimit)
                throw new Exception("Promo code usage limit reached");

            var subtotal = basketItems.Sum(i => i.Product.Price * i.Quantity);
            if (subtotal < promotion.minimumOrderValue)
                throw new Exception($"Minimum order value of {promotion.minimumOrderValue} not met");
        }

        private void ApplyPromoToBasketItems(IEnumerable<BasketItem> basketItems, PromoCode promoCode)
        {
            foreach (var item in basketItems)
            {
                item.PromoCodeId = promoCode.PromoCodeId;
                _context.BasketItems.Update(item);
            }
        }

        private async Task AddPromoItemIfMissing(string userId, PromoCode promoCode, Guid promoProductId)
        {
            var existingPromoItem = await _context.BasketItems
                .Include(b => b.Product)
                .FirstOrDefaultAsync(b => b.UserId == userId && b.Product.IsPromoProduct);

            if (existingPromoItem == null)
            {
                var basketItem = new BasketItem
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ProductId = promoProductId,
                    PromoCodeId = promoCode.PromoCodeId
                };
                _context.BasketItems.Add(basketItem);
            }
        }

       

        public async Task<decimal> CalculateDiscountAsync(string promoCodeInput, decimal subtotal)
        {
            var promoCode = await _context.PromoCodes
                .Include(pc => pc.Promotion)
                .FirstOrDefaultAsync(pc => pc.Code == promoCodeInput);

            if (promoCode?.Promotion == null) return 0;

            return ApplyDiscount(subtotal, promoCode.Promotion.DiscountType, promoCode.Promotion.PromotionValue);
        }

        public decimal CalculateEffectivePrice(decimal originalPrice, Guid? promoCodeId)
        {
            var promoCode = _context.PromoCodes
                .Include(pc => pc.Promotion)
                .FirstOrDefault(pc => pc.PromoCodeId == promoCodeId);

            if (promoCode?.Promotion == null) return originalPrice;

            return ApplyDiscount(originalPrice, promoCode.Promotion.DiscountType, promoCode.Promotion.PromotionValue);
        }

        private static decimal ApplyDiscount(decimal amount, DiscountType discountType, decimal value)
        {
            return discountType switch
            {
                DiscountType.FixedValue => Math.Max(amount - value, 0),
                DiscountType.Percentage => amount - (amount * value / 100),
                _ => amount
            };
        }

        private static PromoCodeResponseDto MapToResponseDto(PromoCode promoCode)
        {
            return new PromoCodeResponseDto
            {
                PromoCodeId = promoCode.PromoCodeId,
                Code = promoCode.Code,
                IsUniqueCode = promoCode.isUniqueCode,
                MaxUsageLimit = promoCode.MaxUsageLimit,
                UsageCount = promoCode.UsageCount,
                UsedOn = promoCode.UsedOn,
                CreatedAt = promoCode.CreatedAt
            };
        }

    }
}