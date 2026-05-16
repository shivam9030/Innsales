using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using InnSales.DataBase;

public class InnSalesDbContextFactory : IDesignTimeDbContextFactory<InnSalesDbContext>
{
    public InnSalesDbContext CreateDbContext(string[] args)
    {
        // var optionsBuilder = new DbContextOptionsBuilder<InnSalesDbContext>();
        // optionsBuilder.UseSqlServer("Server=localhost\\SQLEXPRESS;Database=PPSProj;Trusted_Connection=True;TrustServerCertificate=True;");
 var optionsBuilder = new DbContextOptionsBuilder<InnSalesDbContext>();
    optionsBuilder.UseSqlServer(
    "Server=sqlserver,1433;Database=InnSalesDB;User Id=sa;Password=1shivam2;TrustServerCertificate=True");

        return new InnSalesDbContext(optionsBuilder.Options);
    }
}
 

 