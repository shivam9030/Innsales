using System;
using System.Threading.Tasks;

using InnSales.Domain.Entities;

namespace InnSales.Services
{
    public interface INewsService
    {
        Task<int> CreateNewsAsync(News news);

        Task UpdateNewsAsync(int newsId, News updatedNews);

        Task<News> GetNewsByIdAsync(int newsId);

        Task DeleteNewsAsync(int newsId);
        Task<List<News>> GetAllNewsAsync();

    }
}