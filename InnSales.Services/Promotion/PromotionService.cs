using System;
using InnSales.DataBase;
using InnSales.Domain.Entities;
using InnSales.Common.Enums;
using InnSales.Common.DTO;
using Microsoft.EntityFrameworkCore;

namespace InnSales.Services
{
    public class PromotionService : IPromotionService
    {
        private readonly InnSalesDbContext _context;

        public PromotionService(InnSalesDbContext context)
        {
            _context = context;
        }

        public async Task<PromotionResponseDto> CreatePromotionAsync(PromotionCreateDto dto)
        {
            var promotion = new Promotion
            {
                PromotionId = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                Quantity = dto.Quantity,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                minimumOrderValue = dto.MinimumOrderValue,
                PromotionValue = dto.PromotionValue,
                DiscountType = dto.DiscountType,
                Status = PromotionStatus.Active,
                CreatedAt = DateTime.UtcNow
            };

            _context.Promotions.Add(promotion);
            await _context.SaveChangesAsync();

            return MapToResponseDto(promotion);
        }

        public async Task<PromotionResponseDto> UpdatePromotionAsync(PromotionUpdateDto dto)
        {
            var promotion = await _context.Promotions.FindAsync(dto.PromotionId)
                ?? throw new Exception("Promotion not found");

            promotion.Name = dto.Name;
            promotion.Description = dto.Description;
            promotion.Quantity = dto.Quantity;
            promotion.StartDate = dto.StartDate;
            promotion.EndDate = dto.EndDate;
            promotion.minimumOrderValue = dto.MinimumOrderValue;
            promotion.PromotionValue = dto.PromotionValue;
            promotion.Status = dto.Status;
            promotion.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapToResponseDto(promotion);
        }

        public async Task<bool> DeletePromotionAsync(Guid promotionId)
        {
            var promotion = await _context.Promotions.FindAsync(promotionId);
            if (promotion == null) return false;

            _context.Promotions.Remove(promotion);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<PromotionResponseDto> GetPromotionByIdAsync(Guid promotionId)
        {
            var promotion = await _context.Promotions.FindAsync(promotionId);
            return promotion == null ? null : MapToResponseDto(promotion);
        }

        public async Task<IEnumerable<PromotionResponseDto>> GetAllPromotionsAsync()
        {
            var promotions = await _context.Promotions.ToListAsync();
            return promotions.Select(MapToResponseDto);
        }

        public async Task<bool> ChangePromotionStatusAsync(Guid promotionId, PromotionStatus status)
        {
            var promotion = await _context.Promotions.FindAsync(promotionId);
            if (promotion == null) return false;

            promotion.Status = status;
            promotion.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        // Private mapping method to reduce repetition
        private static PromotionResponseDto MapToResponseDto(Promotion promotion)
        {
            return new PromotionResponseDto
            {
                PromotionId = promotion.PromotionId,
                Name = promotion.Name,
                Description = promotion.Description,
                Quantity = promotion.Quantity,
                StartDate = promotion.StartDate,
                EndDate = promotion.EndDate,
                MinimumOrderValue = promotion.minimumOrderValue,
                PromotionValue = promotion.PromotionValue,
                DiscountType = promotion.DiscountType,
                Status = promotion.Status
            };
        }
    }
}
