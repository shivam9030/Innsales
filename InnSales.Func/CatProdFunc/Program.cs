
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Azure.Core.Serialization;             
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
        var configuration = context.Configuration;

        // Configure WorkerOptions.Serializer to a JsonObjectSerializer with camelCase
        services.Configure<WorkerOptions>(options =>
        {
            options.Serializer = new JsonObjectSerializer(new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            });
        });

        // DbContext
        services.AddDbContext<InnSalesDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Services
        services.AddScoped<IProductService, ProductsService>();
        services.AddScoped<ICategoryService, CategoryService>();

        // AuthHelper as singleton
        services.AddSingleton<IAuthHelper, AuthHelper>();

        // CORS
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
            });
        });
    })
    .Build();

host.Run();
