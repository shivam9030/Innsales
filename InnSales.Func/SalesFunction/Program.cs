
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Azure.Core.Serialization;
using Microsoft.Azure.Functions.Worker;
using InnSales.MappingProfiles;
using InnSales.DataBase;
using InnSales.Services;
using Helper;
using MockEventGrid;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration((context, config) =>
    {
        // Load local settings and environment variables
        config.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
              .AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;
        
services.AddAutoMapper(
        typeof(BasketProfile).Assembly,
        typeof(OrderProfile).Assembly
    );


        // Worker JSON serializer - camelCase and case-insensitive
        services.Configure<WorkerOptions>(options =>
        {
            options.Serializer = new JsonObjectSerializer(new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
                WriteIndented = false
            });
        });

        // DbContext (Scoped) - reads ConnectionStrings:DefaultConnection
        services.AddDbContext<InnSalesDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddSingleton<MockEventGridBroker>();
        services.AddSingleton<IOrderEventPublisher, OrderEventPublisher>();

        services.AddScoped<IPaymentTokenService, PaymentTokenService>(); 
        // Domain/Business Services (Scoped)
        services.AddScoped<IBasketService, BasketService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IPromoCodeService, PromoCodeService>();
        services.AddScoped<IInventoryService, InventoryService>();

        // If this app also hosts Category/Product endpoints, keep these:
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IProductService, ProductsService>();

        // Auth helper (Singleton)
        services.AddSingleton<IAuthHelper, AuthHelper>();

        // HttpClient (recommended for external calls, email, payment, etc.)
        services.AddHttpClient();

        // CORS (default allow-all; adjust for production)
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });
        });
    })
    .Build();

host.Run();
