
// checkoutTest.cs

using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

// ===== Adjust these namespaces to match your solution structure =====
using InnSales.DataBase;
using InnSales.Services;
using InnSales.Common.DTO;
using InnSales.Domain.Entities;

namespace InnSales.Tests.Checkout
{
    /// <summary>
    /// Minimal AutoMapper profile for BasketItem -> BasketItemDto used by BasketService.
    /// If you already have a profile in production code, replace usage accordingly.
    /// </summary>
    internal class BasketMappingProfile : Profile
    {
        public BasketMappingProfile()
        {
            CreateMap<BasketItem, BasketItemDto>()
                .ForMember(d => d.ProductName,    m => m.MapFrom(src => src.Product.Name))
                .ForMember(d => d.ImageUrl,       m => m.MapFrom(src => src.Product.ImageUrl))
                .ForMember(d => d.Price,          m => m.MapFrom(src => src.Product.Price))
                .ForMember(d => d.EffectivePrice, m => m.Ignore()); // computed in service
        }
    }

    /// <summary>
    /// Simple factory to create an InMemory InnSalesDbContext for tests.
    /// </summary>
    internal static class TestDbContextFactory
    {
        public static InnSalesDbContext CreateInMemory(string dbName)
        {
            var options = new DbContextOptionsBuilder<InnSalesDbContext>()
                .UseInMemoryDatabase(dbName)
                .EnableSensitiveDataLogging()
                .Options;

            return new InnSalesDbContext(options);
        }
    }

    /// <summary>
    /// Helper builders for clean test Arrange steps.
    /// </summary>
    internal static class Builders
    {
        public static Product NewProduct(string name, decimal price, bool isPromo = false)
            => new Product
            {
                Id = Guid.NewGuid(),
                Name = name,
                Price = price,
                IsPromoProduct = isPromo,
                ImageUrl = ""
            };

        public static BasketItem NewBasketItem(string userId, Product product, int qty, Guid? promoCodeId = null)
            => new BasketItem
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ProductId = product.Id,
                Quantity = qty,
                Product = product,
                PromoCodeId = promoCodeId
            };
    }

    public class CheckoutTests
    {
        [Fact]
        public async Task CheckoutAsync_WithItems_CallsCreateOrderFromBasket_AndReturnsOrder()
        {
            // Arrange
            var db = TestDbContextFactory.CreateInMemory(nameof(CheckoutAsync_WithItems_CallsCreateOrderFromBasket_AndReturnsOrder));
            var userId = "user-1";

            var p = Builders.NewProduct("X", 100m);
            db.Products.Add(p);
            await db.SaveChangesAsync();

            db.BasketItems.Add(Builders.NewBasketItem(userId, p, qty: 2)); // total = 200
            await db.SaveChangesAsync();

            var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile(new BasketMappingProfile()));
            var mapper = mapperConfig.CreateMapper();

            var orderService = new Mock<IOrderService>();
            var promoService = new Mock<IPromoCodeService>();

            orderService.Setup(s => s.CreateOrderFromBasketAsync(userId, It.IsAny<BasketResponseDto>()))
                        .ReturnsAsync(new OrderDto
                        {
                            Id = Guid.NewGuid(),
                            CustomerId = userId,
                            TotalAmount = 200m,
                            Subtotal = 200m,
                            Discount = 0m,
                            OrderItems = []
                        });

            var sut = new BasketService(db, mapper, orderService.Object, promoService.Object);

            // Act
            var order = await sut.CheckoutAsync(userId);

            // Assert
            Assert.NotNull(order);
            Assert.Equal(userId, order.CustomerId);
            Assert.Equal(200m, order.TotalAmount);

            orderService.Verify(s => s.CreateOrderFromBasketAsync(
                userId,
                It.Is<BasketResponseDto>(b =>
                    b.Items.Count == 1 &&
                    b.TotalPrice == 200m &&
                    b.DiscountAmount == 0m &&
                    b.DiscountedTotal == 200m)
            ), Times.Once);
        }

        [Fact]
        public async Task CheckoutAsync_EmptyBasket_ThrowsInvalidOperation()
        {
            // Arrange
            var db = TestDbContextFactory.CreateInMemory(nameof(CheckoutAsync_EmptyBasket_ThrowsInvalidOperation));
            var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile(new BasketMappingProfile()));
            var mapper = mapperConfig.CreateMapper();

            var orderService = new Mock<IOrderService>();
            var promoService = new Mock<IPromoCodeService>();
            var sut = new BasketService(db, mapper, orderService.Object, promoService.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => sut.CheckoutAsync("user-1"));
            Assert.Equal("Basket is empty.", ex.Message);

            orderService.Verify(s => s.CreateOrderFromBasketAsync(It.IsAny<string>(), It.IsAny<BasketResponseDto>()), Times.Never);
        }

        [Fact]
        public async Task CheckoutAsync_WithPromo_AppliesDiscount_AndPassesDiscountedBasketToOrderService()
        {
            // Arrange
            var db = TestDbContextFactory.CreateInMemory(nameof(CheckoutAsync_WithPromo_AppliesDiscount_AndPassesDiscountedBasketToOrderService));
            var userId = "user-1";

            var promoProduct = Builders.NewProduct("Promo", 100m, isPromo: true);
            var regularProduct = Builders.NewProduct("Regular", 200m, isPromo: false);
            db.Products.AddRange(promoProduct, regularProduct);
            await db.SaveChangesAsync();

            // Add a promo item marked with a promo code and a regular item
            var promoCodeId = Guid.NewGuid();
            db.BasketItems.Add(Builders.NewBasketItem(userId, promoProduct, qty: 1, promoCodeId: promoCodeId));
            db.BasketItems.Add(Builders.NewBasketItem(userId, regularProduct, qty: 1));
            await db.SaveChangesAsync();

            var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile(new BasketMappingProfile()));
            var mapper = mapperConfig.CreateMapper();

            var orderService = new Mock<IOrderService>();
            var promoService = new Mock<IPromoCodeService>();

            // Simulate CalculateEffectivePrice reducing basket total by 10%
            // Basket total: 100 + 200 = 300 -> discounted to 270; discountAmount = 30
            promoService.Setup(s => s.CalculateEffectivePrice(It.IsAny<decimal>(), It.IsAny<Guid?>()))
                        .Returns<decimal, Guid?>((originalTotal, code) => originalTotal * 0.9m);

            OrderDto? capturedOrder = null;
            orderService.Setup(s => s.CreateOrderFromBasketAsync(userId, It.IsAny<BasketResponseDto>()))
                        .ReturnsAsync(() =>
                        {
                            capturedOrder = new OrderDto
                            {
                                Id = Guid.NewGuid(),
                                CustomerId = userId,
                                TotalAmount = 270m,
                                Subtotal = 300m,
                                Discount = 30m,
                                OrderItems = []
                            };
                            return capturedOrder;
                        });

            var sut = new BasketService(db, mapper, orderService.Object, promoService.Object);

            // Act
            var order = await sut.CheckoutAsync(userId);

            // Assert
            Assert.NotNull(order);
            Assert.Equal(userId, order.CustomerId);
            Assert.Equal(270m, order.TotalAmount);
            Assert.Equal(300m, order.Subtotal);
            Assert.Equal(30m, order.Discount);

            orderService.Verify(s => s.CreateOrderFromBasketAsync(
                userId,
                It.Is<BasketResponseDto>(b =>
                    b.TotalPrice == 300m &&
                    b.DiscountedTotal == 270m &&
                    b.DiscountAmount == 30m &&
                    b.Items.Count == 2)
            ), Times.Once);
        }
    }
}
