using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using InnSales.Domain.Entities;
using InnSales.DataBase;

namespace InnSales.Services
{
    public class NewsService : INewsService
    {
        private readonly InnSalesDbContext _context;

        public NewsService(InnSalesDbContext context)
        {
            _context = context;
        }

        public async Task<int> CreateNewsAsync(News news)
        {
            _context.News.Add(news);
            await _context.SaveChangesAsync();
            return news.Id;
        }

        public async Task UpdateNewsAsync(int newsId, News updatedNews)
        {
            var existingNews = await _context.News.FindAsync(newsId);
            if (existingNews != null)
            {
                existingNews.Title = updatedNews.Title;
                existingNews.Description = updatedNews.Description;
                existingNews.PublishedBy = updatedNews.PublishedBy;
                existingNews.PublishedDate = updatedNews.PublishedDate;
                existingNews.EditedBy = updatedNews.EditedBy;
                existingNews.EditedDate = updatedNews.EditedDate;
                existingNews.CreatedBy = updatedNews.CreatedBy;
                existingNews.CreatedDate = updatedNews.CreatedDate;
                existingNews.Category = updatedNews.Category;

                await _context.SaveChangesAsync();
            }
        }

        public async Task<News> GetNewsByIdAsync(int newsId)
        {
            return await _context.News.FindAsync(newsId);
        }

        public async Task DeleteNewsAsync(int newsId)
        {
            var news = await _context.News.FindAsync(newsId);
            if (news != null)
            {
                _context.News.Remove(news);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<News>> GetAllNewsAsync()
        {
            return await _context.News.ToListAsync();
        }
    }
}
