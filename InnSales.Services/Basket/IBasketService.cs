using InnSales.Common.DTO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
public interface IBasketService
{
    Task<BasketResponseDto> GetBasketAsync(string userId);
    Task AddToBasketAsync(AddBasketItemDto dto);
    Task UpdateQuantityAsync(Guid id, int quantity);
    Task RemoveItemAsync(Guid id);
    Task<OrderDto> CheckoutAsync(string userId);
    Task<bool> ApplyPromoCodeAsync(string userId, string promoCode);
}