
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InnSales.Domain.Entities;
using InnSales.Common.DTO;

namespace InnSales.Services
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllAsync();
        Task<IEnumerable<ProductDto>> GetByCategoryAsync(Guid categoryId);
        Task<ProductDto?> GetByIdAsync(Guid id);
        Task<Product> CreateAsync(Product product);
        Task<Product?> UpdateAsync(Guid id, Product updatedProduct);
        Task<bool> DeleteAsync(Guid id);
        Task<Guid> EnsurePromoProductAsync(Guid promoCategoryId);
    }
}
