using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;
using InnSales.DataBase;

public class InnSalesDbContextFactory : IDesignTimeDbContextFactory<InnSalesDbContext>
{
    public InnSalesDbContext CreateDbContext(string[] args)
    {
        // var optionsBuilder = new DbContextOptionsBuilder<InnSalesDbContext>();
        // optionsBuilder.UseSqlServer("Server=localhost\\SQLEXPRESS;Database=PPSProj;Trusted_Connection=True;TrustServerCertificate=True;");
 var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .Build();

var connectionString = configuration.GetConnectionString("DefaultConnection");

var optionsBuilder = new DbContextOptionsBuilder<InnSalesDbContext>();
optionsBuilder.UseNpgsql(connectionString);
    
        return new InnSalesDbContext(optionsBuilder.Options);
    }
}
 

 