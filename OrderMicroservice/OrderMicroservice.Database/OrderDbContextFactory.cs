
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OrderMicroservice.Database
{
    public class OrderDbContextFactory : IDesignTimeDbContextFactory<OrderDbContext>
    {
        public OrderDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<OrderDbContext>();

            // Design-time connection string (SQL Server)
            optionsBuilder.UseSqlServer(
                "Server=localhost;Database=OrderService;User Id=sa;Password=Welcome@1234;TrustServerCertificate=True"
            );

            return new OrderDbContext(optionsBuilder.Options);
        }
    }
}
