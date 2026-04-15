using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using InnSales.DataBase;
using InnSales.Domain.Entities;

namespace InnSales.Services
{
    public class InventoryService: IInventoryService
    {
        private readonly InnSalesDbContext _context;

        public InventoryService(InnSalesDbContext context)
        {
            _context = context;
        }

        public async Task RestockInventoryAsync(Guid orderId)
        {
            var orderItems = await _context.OrderItems
                .Where(oi => oi.OrderId == orderId)
                .ToListAsync();

            foreach (var item in orderItems)
            {
                var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == item.ProductId);
                if (product != null)
                {
                    product.StockQuantity += item.Quantity;
                    _context.Products.Update(product);
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}