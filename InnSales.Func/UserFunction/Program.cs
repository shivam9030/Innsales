using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using InnSales.DataBase;
using InnSales.Services;
using InnSales.Domain.UserManagement.Entities;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults() // Required for isolated worker
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);
        config.AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        // Register DbContext
        services.AddDbContext<InnSalesDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Register ASP.NET Core Identity
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
        })
        .AddEntityFrameworkStores<InnSalesDbContext>()
        .AddDefaultTokenProviders();

        // Register your services
        services.AddScoped<IAuthService, AuthService>();


        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder
                    .AllowAnyOrigin()   
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });
    })
    .Build();

host.Run();
