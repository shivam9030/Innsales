

using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

using InnSales.Services;
using InnSales.DataBase;
using InnSales.Common.Enums;
using InnSales.Common.DTO;
using InnSales.Domain.Entities;

namespace InnSales.Tests
{
    public class PromoCodeServiceTests
    {
        // ---------- Helpers: Sqlite in-memory DbContext (supports transactions) ----------
        private static (InnSalesDbContext ctx, SqliteConnection conn) CreateSqliteInMemory()
        {
            var conn = new SqliteConnection("Data Source=:memory:");
            conn.Open();

            var options = new DbContextOptionsBuilder<InnSalesDbContext>()
                .UseSqlite(conn)
                .EnableSensitiveDataLogging()
                .Options;

            var ctx = new InnSalesDbContext(options);
            ctx.Database.EnsureCreated(); // builds schema based on OnModelCreating
            return (ctx, conn);
        }

        private static PromoCodeService CreateSut(InnSalesDbContext ctx,
            ICategoryService? categorySvc = null,
            IProductService? productSvc = null)
        {
            return new PromoCodeService(ctx,
                categorySvc ?? Mock.Of<ICategoryService>(),
                productSvc ?? Mock.Of<IProductService>());
        }

        // ---------- CRUD ----------

        [Fact]
        public async Task CreatePromoCodeAsync_ShouldAdd_And_MapDto()
        {
            var (ctx, conn) = CreateSqliteInMemory();

            var promotion = TestSeed.CreatePromotion();
            await ctx.SaveAsync(promotion);

            var sut = CreateSut(ctx);

            var dto = new PromoCodeCreateDto
            {
                PromotionId = promotion.PromotionId,
                Code = "HELLO10",
                IsUniqueCode = true,
                MaxUsageLimit = 1
            };

            var created = await sut.CreatePromoCodeAsync(dto);

            created.Should().NotBeNull();
            created.Code.Should().Be("HELLO10");
            created.IsUniqueCode.Should().BeTrue();
            created.MaxUsageLimit.Should().Be(1);
            created.UsageCount.Should().Be(0);
            created.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

            var entity = await ctx.PromoCodes.FirstOrDefaultAsync(pc => pc.Code == "HELLO10");
            entity.Should().NotBeNull();

            conn.Dispose(); ctx.Dispose();
        }

        [Fact]
        public async Task UpdatePromoCodeAsync_ShouldModify_Fields()
        {
            var (ctx, conn) = CreateSqliteInMemory();

            var promotion = TestSeed.CreatePromotion();
            var pc = TestSeed.CreatePromoCode(promotion, code: "UPD10", unique: false, maxLimit: 10);
            await ctx.SaveAsync(promotion, pc);

            var sut = CreateSut(ctx);

            var updated = await sut.UpdatePromoCodeAsync(new PromoCodeUpdateDto
            {
                PromoCodeId = pc.PromoCodeId,
                IsUniqueCode = true,
                MaxUsageLimit = 1
            });

            updated.IsUniqueCode.Should().BeTrue();
            updated.MaxUsageLimit.Should().Be(1);

            var entity = await ctx.PromoCodes.FindAsync(pc.PromoCodeId);
            entity!.isUniqueCode.Should().BeTrue();
            entity.MaxUsageLimit.Should().Be(1);

            conn.Dispose(); ctx.Dispose();
        }

        [Fact]
        public async Task UpdatePromoCodeAsync_ShouldThrow_WhenNotFound()
        {
            var (ctx, conn) = CreateSqliteInMemory();
            var sut = CreateSut(ctx);

            var act = async () => await sut.UpdatePromoCodeAsync(new PromoCodeUpdateDto
            {
                PromoCodeId = Guid.NewGuid(),
                IsUniqueCode = false,
                MaxUsageLimit = 10
            });

            var ex = await Assert.ThrowsAsync<Exception>(act);
            ex.Message.Should().Contain("Promo code not found");

            conn.Dispose(); ctx.Dispose();
        }

        [Fact]
        public async Task DeletePromoCodeAsync_ShouldReturnTrue_WhenDeleted()
        {
            var (ctx, conn) = CreateSqliteInMemory();

            var promotion = TestSeed.CreatePromotion();
            var pc = TestSeed.CreatePromoCode(promotion, code: "DEL10");
            await ctx.SaveAsync(promotion, pc);

            var sut = CreateSut(ctx);

            var result = await sut.DeletePromoCodeAsync(pc.PromoCodeId);
            result.Should().BeTrue();

            var exists = await ctx.PromoCodes.FindAsync(pc.PromoCodeId);
            exists.Should().BeNull();

            conn.Dispose(); ctx.Dispose();
        }

        [Fact]
        public async Task DeletePromoCodeAsync_ShouldReturnFalse_WhenNotFound()
        {
            var (ctx, conn) = CreateSqliteInMemory();
            var sut = CreateSut(ctx);

            var result = await sut.DeletePromoCodeAsync(Guid.NewGuid());
            result.Should().BeFalse();

            conn.Dispose(); ctx.Dispose();
        }

        [Fact]
        public async Task GetPromoCodeByIdAsync_ShouldReturnDto_WhenFound()
        {
            var (ctx, conn) = CreateSqliteInMemory();

            var promotion = TestSeed.CreatePromotion();
            var pc = TestSeed.CreatePromoCode(promotion, code: "FIND10");
            await ctx.SaveAsync(promotion, pc);

            var sut = CreateSut(ctx);

            var dto = await sut.GetPromoCodeByIdAsync(pc.PromoCodeId);
            dto.Should().NotBeNull();
            dto!.Code.Should().Be("FIND10");

            conn.Dispose(); ctx.Dispose();
        }

        [Fact]
        public async Task GetPromoCodesByPromotionAsync_ShouldFilterByPromotionId()
        {
            var (ctx, conn) = CreateSqliteInMemory();

            var p1 = TestSeed.CreatePromotion();
            var p2 = TestSeed.CreatePromotion();

            var pc1 = TestSeed.CreatePromoCode(p1, code: "A1");
            var pc2 = TestSeed.CreatePromoCode(p1, code: "A2");
            var pc3 = TestSeed.CreatePromoCode(p2, code: "B1");

            await ctx.SaveAsync(p1, p2, pc1, pc2, pc3);

            var sut = CreateSut(ctx);

            var list = await sut.GetPromoCodesByPromotionAsync(p1.PromotionId);
            list.Select(x => x.Code).Should().BeEquivalentTo(new[] { "A1", "A2" });

            conn.Dispose(); ctx.Dispose();
        }

        // ---------- ApplyPromoCodeAsync (transactional) ----------

        [Fact]
        public async Task ApplyPromoCodeAsync_ShouldTagBasketItems_And_AddPromoItem_WhenMissing()
        {
            var (ctx, conn) = CreateSqliteInMemory();
            var userId = "user-1";

            // Seed category for regular product (FK required)
            var regularCat = new Category { Id = Guid.NewGuid(), Name = "Regular", IsDeleted = false };
            await ctx.SaveAsync(regularCat);

            // Promotion + promo code
            var promotion = TestSeed.CreatePromotion(discountType: DiscountType.FixedValue, promoValue: 100m, minimumOrder: 0m);
            var pc = TestSeed.CreatePromoCode(promotion, code: "SAVE100");
            await ctx.SaveAsync(promotion, pc);

            // Regular product with a valid CategoryId
            var regular = TestSeed.CreateProduct(price: 500m, isPromo: false);
            regular.CategoryId = regularCat.Id;
            await ctx.SaveAsync(regular);

            // Basket item
            var bi = TestSeed.CreateBasketItem(userId, regular, qty: 2);
            await ctx.SaveAsync(bi);

            // Seed promo category + promo product matching mocked ID
            var promoCat = new Category { Id = Guid.NewGuid(), Name = "Promotions", IsDeleted = false };
            await ctx.SaveAsync(promoCat);

            var promoProductId = Guid.NewGuid();
            var promoProduct = TestSeed.CreateProduct(price: 0m, isPromo: true);
            promoProduct.Id = promoProductId;
            promoProduct.CategoryId = promoCat.Id;
            await ctx.SaveAsync(promoProduct);

            // Mocks return the seeded IDs
            var categorySvc = new Mock<ICategoryService>();
            categorySvc.Setup(s => s.EnsurePromoCategoryAsync()).ReturnsAsync(promoCat.Id);

            var productSvc = new Mock<IProductService>();
            productSvc.Setup(s => s.EnsurePromoProductAsync(promoCat.Id)).ReturnsAsync(promoProductId);

            var sut = CreateSut(ctx, categorySvc.Object, productSvc.Object);

            // Act
            var result = await sut.ApplyPromoCodeAsync(userId, "SAVE100");
            result.Should().BeTrue();

            // Assert basket items tagged
            var items = await ctx.BasketItems.Include(b => b.Product).ToListAsync();
            items.Should().NotBeEmpty();
            items.All(i => i.PromoCodeId == pc.PromoCodeId).Should().BeTrue();

            // Assert promo line exists (with seeded promo product)
            var promoLine = await ctx.BasketItems.Include(b => b.Product)
                .FirstOrDefaultAsync(b => b.UserId == userId && b.Product.IsPromoProduct);
            promoLine.Should().NotBeNull();
            promoLine!.ProductId.Should().Be(promoProductId);

            conn.Dispose(); ctx.Dispose();
        }

        [Fact]
        public async Task ApplyPromoCodeAsync_ShouldNotDuplicatePromoProductLine()
        {
            var (ctx, conn) = CreateSqliteInMemory();
            var userId = "user-2";

            // Categories
            var regularCat = new Category { Id = Guid.NewGuid(), Name = "Regular", IsDeleted = false };
            var promoCat = new Category { Id = Guid.NewGuid(), Name = "Promotions", IsDeleted = false };
            await ctx.SaveAsync(regularCat, promoCat);

            var promotion = TestSeed.CreatePromotion(discountType: DiscountType.Percentage, promoValue: 10m, minimumOrder: 0m);
            var pc = TestSeed.CreatePromoCode(promotion, code: "TENOFF");
            await ctx.SaveAsync(promotion, pc);

            var regular = TestSeed.CreateProduct(price: 200m, isPromo: false);
            regular.CategoryId = regularCat.Id;

            var promoProductId = Guid.NewGuid();
            var promoProduct = TestSeed.CreateProduct(price: 0m, isPromo: true);
            promoProduct.Id = promoProductId;
            promoProduct.CategoryId = promoCat.Id;

            await ctx.SaveAsync(regular, promoProduct);

            // Basket already has a promo product line
            var promoLine = TestSeed.CreateBasketItem(userId, promoProduct, qty: 1);
            var regularLine = TestSeed.CreateBasketItem(userId, regular, qty: 1);
            await ctx.SaveAsync(promoLine, regularLine);

            var categorySvc = new Mock<ICategoryService>();
            categorySvc.Setup(s => s.EnsurePromoCategoryAsync()).ReturnsAsync(promoCat.Id);

            var productSvc = new Mock<IProductService>();
            productSvc.Setup(s => s.EnsurePromoProductAsync(promoCat.Id)).ReturnsAsync(promoProductId);

            var sut = CreateSut(ctx, categorySvc.Object, productSvc.Object);

            var result = await sut.ApplyPromoCodeAsync(userId, "TENOFF");
            result.Should().BeTrue();

            var promoLines = await ctx.BasketItems.Include(b => b.Product)
                .Where(b => b.UserId == userId && b.Product.IsPromoProduct)
                .ToListAsync();

            promoLines.Count.Should().Be(1); // no duplicates
            promoLines[0].ProductId.Should().Be(promoProductId);

            conn.Dispose(); ctx.Dispose();
        }

        [Fact]
        public async Task ApplyPromoCodeAsync_ShouldThrow_WhenBasketIsEmpty()
        {
            var (ctx, conn) = CreateSqliteInMemory();
            var userId = "user-empty";

            var promotion = TestSeed.CreatePromotion();
            var pc = TestSeed.CreatePromoCode(promotion, code: "CODE10");
            await ctx.SaveAsync(promotion, pc);

            var sut = CreateSut(ctx);

            var act = async () => await sut.ApplyPromoCodeAsync(userId, "CODE10");
            var ex = await Assert.ThrowsAsync<Exception>(act);
            ex.Message.Should().Contain("Basket is empty");

            conn.Dispose(); ctx.Dispose();
        }

        [Fact]
        public async Task ApplyPromoCodeAsync_ShouldThrow_WhenInvalidCode()
        {
            var (ctx, conn) = CreateSqliteInMemory();
            var userId = "user-3";

            // Regular category + product + basket
            var regularCat = new Category { Id = Guid.NewGuid(), Name = "Regular", IsDeleted = false };
            await ctx.SaveAsync(regularCat);

            var regular = TestSeed.CreateProduct(price: 100m);
            regular.CategoryId = regularCat.Id;
            await ctx.SaveAsync(regular);

            var bi = TestSeed.CreateBasketItem(userId, regular, qty: 1);
            await ctx.SaveAsync(bi);

            var sut = CreateSut(ctx);

            var act = async () => await sut.ApplyPromoCodeAsync(userId, "NOPE");
            var ex = await Assert.ThrowsAsync<Exception>(act);
            ex.Message.Should().Contain("Invalid promo code");

            conn.Dispose(); ctx.Dispose();
        }

        [Fact]
        public async Task ApplyPromoCodeAsync_ShouldThrow_WhenInactiveOrExpired()
        {
            var (ctx, conn) = CreateSqliteInMemory();
            var userId = "user-4";

            var promotion = TestSeed.CreatePromotion(
                status: PromotionStatus.Inactive,
                start: DateTime.UtcNow.AddDays(-2),
                end: DateTime.UtcNow.AddDays(-1)); // expired window
            var pc = TestSeed.CreatePromoCode(promotion, code: "ANY");
            await ctx.SaveAsync(promotion, pc);

            var regularCat = new Category { Id = Guid.NewGuid(), Name = "Regular", IsDeleted = false };
            await ctx.SaveAsync(regularCat);

            var regular = TestSeed.CreateProduct(price: 100m);
            regular.CategoryId = regularCat.Id;
            await ctx.SaveAsync(regular);

            var bi = TestSeed.CreateBasketItem(userId, regular, qty: 1);
            await ctx.SaveAsync(bi);

            var sut = CreateSut(ctx);

            var act = async () => await sut.ApplyPromoCodeAsync(userId, "ANY");
            var ex = await Assert.ThrowsAsync<Exception>(act);
            ex.Message.Should().Contain("expired or inactive");

            conn.Dispose(); ctx.Dispose();
        }

        [Fact]
        public async Task ApplyPromoCodeAsync_ShouldThrow_WhenUniqueAlreadyUsed()
        {
            var (ctx, conn) = CreateSqliteInMemory();
            var userId = "user-5";

            var promotion = TestSeed.CreatePromotion(status: PromotionStatus.Active);
            var pc = TestSeed.CreatePromoCode(promotion, code: "UNIQ", unique: true, maxLimit: 1, usageCount: 1);
            await ctx.SaveAsync(promotion, pc);

            var regularCat = new Category { Id = Guid.NewGuid(), Name = "Regular", IsDeleted = false };
            await ctx.SaveAsync(regularCat);

            var regular = TestSeed.CreateProduct(price: 100m);
            regular.CategoryId = regularCat.Id;
            await ctx.SaveAsync(regular);

            var bi = TestSeed.CreateBasketItem(userId, regular, qty: 1);
            await ctx.SaveAsync(bi);

            var sut = CreateSut(ctx);

            var act = async () => await sut.ApplyPromoCodeAsync(userId, "UNIQ");
            var ex = await Assert.ThrowsAsync<Exception>(act);
            ex.Message.Should().Contain("already used");

            conn.Dispose(); ctx.Dispose();
        }

        [Fact]
        public async Task ApplyPromoCodeAsync_ShouldThrow_WhenUsageLimitReached()
        {
            var (ctx, conn) = CreateSqliteInMemory();
            var userId = "user-6";

            var promotion = TestSeed.CreatePromotion();
            var pc = TestSeed.CreatePromoCode(promotion, code: "LIMIT", unique: false, maxLimit: 2, usageCount: 2);
            await ctx.SaveAsync(promotion, pc);

            var regularCat = new Category { Id = Guid.NewGuid(), Name = "Regular", IsDeleted = false };
            await ctx.SaveAsync(regularCat);

            var regular = TestSeed.CreateProduct(price: 100m);
            regular.CategoryId = regularCat.Id;
            await ctx.SaveAsync(regular);

            var bi = TestSeed.CreateBasketItem(userId, regular, qty: 1);
            await ctx.SaveAsync(bi);

            var sut = CreateSut(ctx);

            var act = async () => await sut.ApplyPromoCodeAsync(userId, "LIMIT");
            var ex = await Assert.ThrowsAsync<Exception>(act);
            ex.Message.Should().Contain("usage limit reached");

            conn.Dispose(); ctx.Dispose();
        }

        [Fact]
        public async Task ApplyPromoCodeAsync_ShouldThrow_WhenMinimumOrderNotMet()
        {
            var (ctx, conn) = CreateSqliteInMemory();
            var userId = "user-7";

            var promotion = TestSeed.CreatePromotion(minimumOrder: 500m);
            var pc = TestSeed.CreatePromoCode(promotion, code: "MIN500");
            await ctx.SaveAsync(promotion, pc);

            var regularCat = new Category { Id = Guid.NewGuid(), Name = "Regular", IsDeleted = false };
            await ctx.SaveAsync(regularCat);

            var cheap = TestSeed.CreateProduct(price: 100m);
            cheap.CategoryId = regularCat.Id;
            await ctx.SaveAsync(cheap);

            var bi = TestSeed.CreateBasketItem(userId, cheap, qty: 3); // subtotal 300 < 500
            await ctx.SaveAsync(bi);

            var sut = CreateSut(ctx);

            var act = async () => await sut.ApplyPromoCodeAsync(userId, "MIN500");
            var ex = await Assert.ThrowsAsync<Exception>(act);
            ex.Message.Should().Contain("Minimum order value");

            conn.Dispose(); ctx.Dispose();
        }

        [Fact]
        public async Task ApplyPromoCodeAsync_ShouldNotIncrementUsage_OnApply()
        {
            var (ctx, conn) = CreateSqliteInMemory();
            var userId = "user-apply";

            var promotion = TestSeed.CreatePromotion(status: PromotionStatus.Active, minimumOrder: 0m);
            var promo = TestSeed.CreatePromoCode(promotion, code: "APPLY10", unique: false, maxLimit: 100, usageCount: 0);
            await ctx.SaveAsync(promotion, promo);

            var regularCat = new Category { Id = Guid.NewGuid(), Name = "Regular", IsDeleted = false };
            await ctx.SaveAsync(regularCat);

            var product = TestSeed.CreateProduct(price: 200m);
            product.CategoryId = regularCat.Id;
            await ctx.SaveAsync(product);

            var bi = TestSeed.CreateBasketItem(userId, product, qty: 1);
            await ctx.SaveAsync(bi);

            var promoCat = new Category { Id = Guid.NewGuid(), Name = "Promotions", IsDeleted = false };
            await ctx.SaveAsync(promoCat);

            var promoProductId = Guid.NewGuid();
            var promoProduct = TestSeed.CreateProduct(price: 0m, isPromo: true);
            promoProduct.Id = promoProductId;
            promoProduct.CategoryId = promoCat.Id;
            await ctx.SaveAsync(promoProduct);

            var categorySvc = new Mock<ICategoryService>();
            categorySvc.Setup(s => s.EnsurePromoCategoryAsync()).ReturnsAsync(promoCat.Id);
            var productSvc = new Mock<IProductService>();
            productSvc.Setup(s => s.EnsurePromoProductAsync(promoCat.Id)).ReturnsAsync(promoProductId);

            var sut = CreateSut(ctx, categorySvc.Object, productSvc.Object);

            (await sut.ApplyPromoCodeAsync(userId, "APPLY10")).Should().BeTrue();

            var reloaded = await ctx.PromoCodes.FindAsync(promo.PromoCodeId);
            reloaded!.UsageCount.Should().Be(0);
            reloaded.UsedOn.Should().BeNull();

            conn.Dispose(); ctx.Dispose();
        }

        // ---------- Calculate / Effective Price ----------

        [Fact]
        public async Task CalculateDiscountAsync_Percentage_ReturnsEffectivePrice()
        {
            var (ctx, conn) = CreateSqliteInMemory();

            var p = TestSeed.CreatePromotion(discountType: DiscountType.Percentage, promoValue: 25m);
            var pc = TestSeed.CreatePromoCode(p, code: "NEWYEAR25");
            await ctx.SaveAsync(p, pc);

            var sut = CreateSut(ctx);

            var effective = await sut.CalculateDiscountAsync("NEWYEAR25", 1000m);
            effective.Should().Be(750m); // 25% off

            conn.Dispose(); ctx.Dispose();
        }

        [Fact]
        public async Task CalculateDiscountAsync_FixedValue_ClampedToZero()
        {
            var (ctx, conn) = CreateSqliteInMemory();

            var p = TestSeed.CreatePromotion(discountType: DiscountType.FixedValue, promoValue: 500m);
            var pc = TestSeed.CreatePromoCode(p, code: "SAVE500");
            await ctx.SaveAsync(p, pc);

            var sut = CreateSut(ctx);

            var effective = await sut.CalculateDiscountAsync("SAVE500", 400m);
            effective.Should().Be(0m); // max(amount - value, 0)

            conn.Dispose(); ctx.Dispose();
        }

        [Fact]
        public void CalculateEffectivePrice_ShouldReturnOriginal_WhenPromoMissing()
        {
            var (ctx, conn) = CreateSqliteInMemory();
            var sut = CreateSut(ctx);

            var original = sut.CalculateEffectivePrice(999m, promoCodeId: Guid.NewGuid());
            original.Should().Be(999m);

            conn.Dispose(); ctx.Dispose();
        }

        [Fact]
        public void CalculateEffectivePrice_Percentage_AppliesCorrectly()
        {
            var (ctx, conn) = CreateSqliteInMemory();

            var p = TestSeed.CreatePromotion(discountType: DiscountType.Percentage, promoValue: 10m);
            var pc = TestSeed.CreatePromoCode(p, code: "ANY");
            ctx.Add(p);
            ctx.Add(pc);
            ctx.SaveChanges();

            var sut = CreateSut(ctx);

            var effective = sut.CalculateEffectivePrice(200m, pc.PromoCodeId);
            effective.Should().Be(180m); // 10% off

            conn.Dispose(); ctx.Dispose();
        }
    }

    // ---------- Minimal helpers embedded in the same file (keep single-file style) ----------

    internal static class TestSeed
    {
        public static Promotion CreatePromotion(
            Guid? id = null,
            DiscountType discountType = DiscountType.Percentage,
            decimal promoValue = 10m,
            PromotionStatus status = PromotionStatus.Active,
            decimal minimumOrder = 0m,
            DateTime? start = null,
            DateTime? end = null)
        {
            return new Promotion
            {
                PromotionId = id ?? Guid.NewGuid(),
                Name = "Test Promo",
                Description = "For tests",
                Quantity = 0,
                StartDate = start ?? DateTime.UtcNow.AddDays(-1),
                EndDate = end ?? DateTime.UtcNow.AddDays(1),
                minimumOrderValue = minimumOrder,
                PromotionValue = promoValue,
                DiscountType = discountType,
                Status = status,
                CreatedAt = DateTime.UtcNow,
                PromoCodes = new System.Collections.Generic.List<PromoCode>()
            };
        }

        public static PromoCode CreatePromoCode(Promotion promotion, string code = "CODE10", bool unique = false, int maxLimit = 100, int usageCount = 0)
        {
            var pc = new PromoCode
            {
                PromoCodeId = Guid.NewGuid(),
                PromotionId = promotion.PromotionId,
                Code = code,
                isUniqueCode = unique,
                MaxUsageLimit = maxLimit,
                UsageCount = usageCount,
                CreatedAt = DateTime.UtcNow,
                Promotion = promotion
            };
            promotion.PromoCodes!.Add(pc);
            return pc;
        }

        public static Product CreateProduct(decimal price = 100m, bool isPromo = false)
        {
            return new Product
            {
                Id = Guid.NewGuid(),
                Name = isPromo ? "PromoCode Item" : "Regular Product",
                Description = "",
                Price = price,
                StockQuantity = isPromo ? null : 10,
                IsDeleted = false,
                // CategoryId must be set in the test after creating a Category (FK required)
                IsPromoProduct = isPromo
            };
        }

        public static BasketItem CreateBasketItem(string userId, Product product, int qty = 1, Guid? promoCodeId = null)
        {
            return new BasketItem
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ProductId = product.Id,
                Quantity = qty,
                Product = product,
                PromoCodeId = promoCodeId
                // Leave navigation PromoCode unset; FK optionality is controlled by PromoCodeId (Guid?)
            };
        }

        public static async Task SaveAsync(this InnSalesDbContext ctx, params object[] entities)
        {
            foreach (var e in entities) ctx.Add(e);
            await ctx.SaveChangesAsync();
        }
    }
}
