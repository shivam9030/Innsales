using System;
using System.Threading.Tasks;
using InnSales.Domain.Entities;

namespace InnSales.Services
{
    public interface IInventoryService
    {
        Task RestockInventoryAsync(Guid orderId);
    }
}