
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using InnSales.Domain.Entities;
using InnSales.DataBase;

namespace InnSales.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly InnSalesDbContext _context;

        public CategoryService(InnSalesDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> CreateCategoryAsync(Category category)
        {
            if (category == null) throw new ArgumentNullException(nameof(category));

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return category.Id;
        }

        public async Task UpdateCategoryAsync(Guid categoryId, Category updatedCategory)
        {
            var category = await GetCategoryEntityAsync(categoryId);
            category.Name = updatedCategory.Name;
            category.Description = updatedCategory.Description;
            category.ImageUrl = updatedCategory.ImageUrl;

            await _context.SaveChangesAsync();
        }

        public async Task<Category> GetCategoryByIdAsync(Guid categoryId)
        {
            return await GetCategoryEntityAsync(categoryId);
        }

        public async Task DeleteCategoryAsync(Guid categoryId)
        {
            var category = await GetCategoryEntityAsync(categoryId);
            category.IsDeleted = true;
            await _context.SaveChangesAsync();
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories
                .Where(c => !c.IsDeleted && c.Name != "Promotions")
                .ToListAsync();
        }

        public async Task<Guid> EnsurePromoCategoryAsync()
        {
            var promoCategory = await _context.Categories
                .FirstOrDefaultAsync(c => c.Name == "Promotions");

            if (promoCategory == null)
            {
                promoCategory = new Category
                {
                    Id = Guid.NewGuid(),
                    Name = "Promotions",
                    Description = "Promo code items",
                    IsDeleted = false
                };

                _context.Categories.Add(promoCategory);
                await _context.SaveChangesAsync();
            }

            return promoCategory.Id;
        }

        // DRY: Common method for fetching category or throwing exception
        private async Task<Category> GetCategoryEntityAsync(Guid categoryId)
        {
            var category = await _context.Categories.FindAsync(categoryId);
            if (category == null)
                throw new KeyNotFoundException("Category not found");
            return category;
        }
    }
}
