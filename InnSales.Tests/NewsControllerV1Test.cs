
using Microsoft.EntityFrameworkCore;
using InnSales.Domain.Entities;
using InnSales.DataBase;
using InnSales.Services;

namespace InnSales.Tests
{
    public class NewsServiceTests
    {
        private InnSalesDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<InnSalesDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new InnSalesDbContext(options);
        }

        [Fact]
        public async Task CreateNewsAsync_ShouldAddNews()
        {
            var context = GetDbContext();
            var service = new NewsService(context);

            var news = new News
            {
                Title = "Test News",
                Description = "Test Description",
                PublishedBy = "Admin",
                PublishedDate = DateTime.UtcNow,
                CreatedBy = "Admin",
                CreatedDate = DateTime.UtcNow,
                Category = "General"
            };

            var id = await service.CreateNewsAsync(news);
            var result = await context.News.FindAsync(id);

            Assert.NotNull(result);
            Assert.Equal("Test News", result.Title);
        }

        [Fact]
        public async Task GetNewsByIdAsync_ShouldReturnNews()
        {
            var context = GetDbContext();
            var service = new NewsService(context);

            var news = new News
            {
                Title = "Sample News",
                Description = "Sample Description",
                PublishedBy = "Editor",
                PublishedDate = DateTime.UtcNow,
                CreatedBy = "Editor",
                CreatedDate = DateTime.UtcNow,
                Category = "Tech"
            };

            context.News.Add(news);
            await context.SaveChangesAsync();

            var result = await service.GetNewsByIdAsync(news.Id);

            Assert.NotNull(result);
            Assert.Equal("Sample News", result.Title);
        }

        [Fact]
        public async Task UpdateNewsAsync_ShouldModifyNews()
        {
            var context = GetDbContext();
            var service = new NewsService(context);

            var news = new News
            {
                Title = "Old Title",
                Description = "Old Description",
                PublishedBy = "User",
                PublishedDate = DateTime.UtcNow,
                CreatedBy = "User",
                CreatedDate = DateTime.UtcNow,
                Category = "Old"
            };

            context.News.Add(news);
            await context.SaveChangesAsync();

            var updatedNews = new News
            {
                Title = "New Title",
                Description = "New Description",
                PublishedBy = "User",
                PublishedDate = DateTime.UtcNow,
                EditedBy = "Editor",
                EditedDate = DateTime.UtcNow,
                CreatedBy = "User",
                CreatedDate = news.CreatedDate,
                Category = "Updated"
            };

            await service.UpdateNewsAsync(news.Id, updatedNews);
            var result = await context.News.FindAsync(news.Id);

            Assert.Equal("New Title", result.Title);
            Assert.Equal("Updated", result.Category);
        }

        [Fact]
        public async Task DeleteNewsAsync_ShouldRemoveNews()
        {
            var context = GetDbContext();
            var service = new NewsService(context);

            var news = new News
            {
                Title = "Delete Me",
                Description = "To be deleted",
                PublishedBy = "Admin",
                PublishedDate = DateTime.UtcNow,
                CreatedBy = "Admin",
                CreatedDate = DateTime.UtcNow,
                Category = "Trash"
            };

            context.News.Add(news);
            await context.SaveChangesAsync();

            await service.DeleteNewsAsync(news.Id);
            var result = await context.News.FindAsync(news.Id);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllNewsAsync_ShouldReturnAllNews()
        {
            var context = GetDbContext();
            var service = new NewsService(context);

            context.News.AddRange(new List<News>
            {
                new News { Title = "News 1", Description = "Desc 1", PublishedBy = "A", PublishedDate = DateTime.UtcNow, CreatedBy = "A", CreatedDate = DateTime.UtcNow, Category = "General" },
                new News { Title = "News 2", Description = "Desc 2", PublishedBy = "B", PublishedDate = DateTime.UtcNow, CreatedBy = "B", CreatedDate = DateTime.UtcNow, Category = "Tech" }
            });

            await context.SaveChangesAsync();

            var result = await service.GetAllNewsAsync();

            Assert.Equal(2, result.Count);
        }
    }
}
