using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using InnSales.DataBase;
using InnSales.Common.Enums;

namespace InnSales.Services
{
    public class PaymentTimeoutService : BackgroundService, IPaymentTimeoutService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public PaymentTimeoutService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task CheckAndCancelTimedOutPaymentsAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<InnSalesDbContext>();

            var timeoutThreshold = DateTime.UtcNow.AddMinutes(-15);
            var pendingOrders = await context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.PaymentStatus == PaymentStatus.Pending && o.OrderDate < timeoutThreshold)
                .ToListAsync();

            foreach (var order in pendingOrders)
            {
                foreach (var item in order.OrderItems)
                {
                    var product = await context.Products.FirstOrDefaultAsync(p => p.Id == item.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity += item.Quantity;
                        context.Products.Update(product);
                    }
                }

                order.PaymentStatus = PaymentStatus.Failed;
                order.OrderStatus = OrderStatus.Cancelled;
                context.Orders.Update(order);

                var payment = await context.Payments.FirstOrDefaultAsync(p => p.OrderId == order.Id);
                if (payment != null)
                {
                    payment.PaymentStatus = PaymentStatus.Failed;
                    context.Payments.Update(payment);
                }
            }

            await context.SaveChangesAsync();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await CheckAndCancelTimedOutPaymentsAsync();
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}