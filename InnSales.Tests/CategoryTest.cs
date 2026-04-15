using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using InnSales.DataBase;
using InnSales.Domain.Entities;
using InnSales.Services;
using Xunit;

namespace InnSales.Tests
{
    public class CategoryServiceTests
    {
        private InnSalesDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<InnSalesDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new InnSalesDbContext(options);
        }

        [Fact]
        public async Task CreateCategoryAsync_ShouldAddCategory()
        {
            var context = GetDbContext();
            var service = new CategoryService(context);

            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = "Electronics",
                Description = "Electronic items",
                ImageUrl = "http://example.com/image.jpg"
            };

            var id = await service.CreateCategoryAsync(category);
            var result = await context.Categories.FindAsync(id);

            Assert.NotNull(result);
            Assert.Equal("Electronics", result.Name);
        }

        [Fact]
        public async Task GetCategoryByIdAsync_ShouldReturnCategory()
        {
            var context = GetDbContext();
            var service = new CategoryService(context);

            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = "Books",
                Description = "All kinds of books"
            };

            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var result = await service.GetCategoryByIdAsync(category.Id);

            Assert.NotNull(result);
            Assert.Equal("Books", result.Name);
        }

        [Fact]
        public async Task UpdateCategoryAsync_ShouldModifyCategory()
        {
            var context = GetDbContext();
            var service = new CategoryService(context);

            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = "Old Name",
                Description = "Old Description"
            };

            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var updatedCategory = new Category
            {
                Name = "New Name",
                Description = "New Description",
                ImageUrl = "http://example.com/new.jpg"
            };

            await service.UpdateCategoryAsync(category.Id, updatedCategory);
            var result = await context.Categories.FindAsync(category.Id);

            Assert.Equal("New Name", result.Name);
            Assert.Equal("New Description", result.Description);
            Assert.Equal("http://example.com/new.jpg", result.ImageUrl);
        }

        [Fact]
        public async Task DeleteCategoryAsync_ShouldSoftDeleteCategory()
        {
            var context = GetDbContext();
            var service = new CategoryService(context);

            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = "To Be Deleted"
            };

            context.Categories.Add(category);
            await context.SaveChangesAsync();

            await service.DeleteCategoryAsync(category.Id);
            var result = await context.Categories.FindAsync(category.Id);

            Assert.NotNull(result);
            Assert.True(result.IsDeleted);
        }

        [Fact]
        public async Task GetAllCategoriesAsync_ShouldReturnAllNonDeletedCategories()
        {
            var context = GetDbContext();
            var service = new CategoryService(context);

            var cat1 = new Category { Id = Guid.NewGuid(), Name = "Cat 1" };
            var cat2 = new Category { Id = Guid.NewGuid(), Name = "Cat 2" };
            var cat3 = new Category { Id = Guid.NewGuid(), Name = "Deleted Cat", IsDeleted = true };

            context.Categories.AddRange(cat1, cat2, cat3);
            await context.SaveChangesAsync();

            var result = await service.GetAllCategoriesAsync();

            Assert.Equal(2, result.Count);
            Assert.DoesNotContain(result, c => c.IsDeleted);
        }
    }
}