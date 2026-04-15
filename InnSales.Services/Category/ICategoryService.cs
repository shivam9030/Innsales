
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InnSales.Domain.Entities;

namespace InnSales.Services
{
    public interface ICategoryService
    {
        Task<Guid> CreateCategoryAsync(Category category);
        Task UpdateCategoryAsync(Guid categoryId, Category updatedCategory);
        Task<Category> GetCategoryByIdAsync(Guid categoryId);
        Task DeleteCategoryAsync(Guid categoryId);
        Task<List<Category>> GetAllCategoriesAsync();
        Task<Guid> EnsurePromoCategoryAsync();
    }
}
