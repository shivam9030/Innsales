
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using InnSales.DataBase;
using InnSales.Domain.Entities;
using InnSales.Common.Enums;
using InnSales.Common.DTO;
namespace InnSales.Services
{
    public class BasketService : IBasketService
    {
        private readonly InnSalesDbContext _context;
        private readonly IMapper _mapper;
        private readonly IOrderService _orderService;
        private readonly IPromoCodeService _promocodeService;

        public BasketService(InnSalesDbContext context, IMapper mapper, IOrderService orderService,
                             IPromoCodeService promoCodeService)
        {
            _context = context;
            _mapper = mapper;
            _orderService = orderService;
            _promocodeService = promoCodeService;
        }

public async Task<BasketResponseDto> GetBasketAsync(string userId)
{
    var items = await _context.BasketItems
        .Include(b => b.Product)
        .Where(b => b.UserId == userId)
        .ToListAsync();

    var basketDtos = _mapper.Map<List<BasketItemDto>>(items);

    // Calculate original basket total
    decimal totalPrice = basketDtos.Sum(dto => dto.Price * dto.Quantity);
    decimal discountedTotal = totalPrice;

    var promoCodeId = items.FirstOrDefault(i => i.PromoCodeId.HasValue)?.PromoCodeId;

    if (promoCodeId.HasValue)
    {
        discountedTotal = _promocodeService.CalculateEffectivePrice(totalPrice, promoCodeId.Value);
    }

    decimal discountAmount = totalPrice - discountedTotal;

    // Apply discount only to promo products
    foreach (var dto in basketDtos)
    {
        var basketItem = items.First(i => i.Id == dto.Id);

        if (basketItem.PromoCodeId.HasValue && basketItem.Product.IsPromoProduct)
        {
           
            dto.EffectivePrice = discountAmount;
        }
        else
        {
            // Keep original price for non-promo products
            dto.EffectivePrice = dto.Price * dto.Quantity;
        }
    }

    return new BasketResponseDto
    {
        Items = basketDtos,
        TotalPrice = totalPrice,
        DiscountedTotal = discountedTotal,
        DiscountAmount = discountAmount
    };
}

        public async Task AddToBasketAsync(AddBasketItemDto dto)
        {
            var existing = await _context.BasketItems
                .FirstOrDefaultAsync(b => b.UserId == dto.UserId && b.ProductId == dto.ProductId);

            if (existing != null)
            {
                existing.Quantity += dto.Quantity;
            }
            else
            {
                var item = new BasketItem
                {
                    Id = Guid.NewGuid(),
                    UserId = dto.UserId,
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity
                };
                _context.BasketItems.Add(item);
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdateQuantityAsync(Guid id, int quantity)
        {
            var item = await _context.BasketItems.FindAsync(id);
            if (item == null) throw new Exception("Item not found");

            item.Quantity = quantity;
            await _context.SaveChangesAsync();

            await ValidatePromoAfterBasketChangeAsync(item.UserId);
        }

        public async Task RemoveItemAsync(Guid id)
        {
            var item = await _context.BasketItems.FindAsync(id);
            if (item == null) throw new Exception("Item not found");

            _context.BasketItems.Remove(item);
            await _context.SaveChangesAsync();
            await ValidatePromoAfterBasketChangeAsync(item.UserId);
        }


public async Task<OrderDto> CheckoutAsync(string userId)
{
    // 1. Get basket for the user
    BasketResponseDto basket = await GetBasketAsync(userId);

    if (basket == null || !basket.Items.Any())
        throw new InvalidOperationException("Basket is empty.");

    // 2. Pass the basket to the order service
    return await _orderService.CreateOrderFromBasketAsync(userId, basket);
}


        public async Task<bool> ApplyPromoCodeAsync(string userId, string promoCode)
        {
            return await _promocodeService.ApplyPromoCodeAsync(userId, promoCode);
        }

        private async Task<string> ValidatePromoAfterBasketChangeAsync(string userId)
        {
            var basketItems = await _context.BasketItems
                .Include(b => b.Product)
                .Where(b => b.UserId == userId)
                .ToListAsync();
            if (!basketItems.Any()) return null;
            var subtotal = basketItems.Sum(item => item.Product.Price * item.Quantity);
            // Check if any promo is applied
            var appliedPromoId = basketItems.FirstOrDefault(b => b.PromoCodeId != null)?.PromoCodeId;
            if (appliedPromoId == null) return null;
            var promoCode = await _context.PromoCodes
                .Include(pc => pc.Promotion)
                .FirstOrDefaultAsync(pc => pc.PromoCodeId == appliedPromoId);
            if (promoCode != null && subtotal < promoCode.Promotion.minimumOrderValue)
            {

                foreach (var item in basketItems)
                {
                    item.PromoCodeId = null;
                }
                var promoItem = basketItems.FirstOrDefault(b => b.Product.IsPromoProduct);
                if (promoItem != null)
                {
                    _context.BasketItems.Remove(promoItem);
                }
                await _context.SaveChangesAsync();
                return "Promo code removed due to insufficient order value.";

            }
            return null;
        }

    }
}

