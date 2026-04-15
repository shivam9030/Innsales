
using Microsoft.EntityFrameworkCore;
using InnSales.Domain.Entities;
using InnSales.DataBase;
using InnSales.Common.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InnSales.Services
{
    public class ProductsService : IProductService
    {
        private readonly InnSalesDbContext _context;

        public ProductsService(InnSalesDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProductDto>> GetAllAsync()
        {
            var products = await _context.Products
                .Where(p => !p.IsDeleted)
                .ToListAsync();

            return products.Select(MapToDto);
        }

        public async Task<IEnumerable<ProductDto>> GetByCategoryAsync(Guid categoryId)
        {
            var products = await _context.Products
                .Where(p => !p.IsDeleted && p.CategoryId == categoryId)
                .ToListAsync();

            return products.Select(MapToDto);
        }

        public async Task<ProductDto?> GetByIdAsync(Guid id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            return product == null ? null : MapToDto(product);
        }

        public async Task<Product> CreateAsync(Product product)
        {
            if (product == null) throw new ArgumentNullException(nameof(product));

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<Product?> UpdateAsync(Guid id, Product updatedProduct)
        {
            var existingProduct = await _context.Products.FindAsync(id);
            if (existingProduct == null) return null;

            existingProduct.Name = updatedProduct.Name;
            existingProduct.Description = updatedProduct.Description;
            existingProduct.ImageUrl = updatedProduct.ImageUrl;
            existingProduct.Price = updatedProduct.Price;
            existingProduct.StockQuantity = updatedProduct.StockQuantity;
            existingProduct.CategoryId = updatedProduct.CategoryId;

            await _context.SaveChangesAsync();
            return existingProduct;
        }

        public async Task<bool> DeleteAsync(Guid productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return false;

            product.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Guid> EnsurePromoProductAsync(Guid promoCategoryId)
        {
            var promoProduct = await _context.Products
                .FirstOrDefaultAsync(p => p.IsPromoProduct);

            if (promoProduct == null)
            {
                promoProduct = new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "PromoCode Item",
                    Description = "Represents applied promo code",
                    Price = 0,
                    StockQuantity = null,
                    CategoryId = promoCategoryId,
                    IsPromoProduct = true
                };

                _context.Products.Add(promoProduct);
                await _context.SaveChangesAsync();
            }

            return promoProduct.Id;
        }

        // DRY: Common mapping logic
        private ProductDto MapToDto(Product product)
        {
            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                ImageUrl = product.IsPromoProduct ? null : product.ImageUrl,
                Price = product.Price,
                StockQuantity = product.IsPromoProduct ? null : product.StockQuantity,
                CategoryId = product.CategoryId,
                AvailabilityStatus = product.IsPromoProduct ? null : product.AvailabilityStatus,
                IsPromoProduct = product.IsPromoProduct
            };
        }
    }
}
