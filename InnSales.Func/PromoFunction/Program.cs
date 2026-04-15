
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Azure.Functions.Worker;

using InnSales.DataBase;
using InnSales.Services;
using Helper;

var host = new HostBuilder()
   
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
              .AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        var cfg = context.Configuration;

        // --- Database ---
        var connString = cfg.GetConnectionString("DefaultConnection")
                         ?? cfg["ConnectionStrings:DefaultConnection"];

        services.AddDbContext<InnSalesDbContext>(options =>
            options.UseSqlServer(connString));

        // --- Domain services ---
        services.AddScoped<IPromotionService, PromotionService>();
        services.AddScoped<IPromoCodeService, PromoCodeService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IProductService, ProductsService>();

     
        services.AddScoped<IAuthHelper, AuthHelper>();

       
    })
    .Build();

host.Run();
