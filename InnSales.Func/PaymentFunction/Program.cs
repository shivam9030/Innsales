using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using InnSales.DataBase;
using InnSales.Services;
using InnSales.MappingProfiles;
using Microsoft.Extensions.Configuration;
using Stripe;
using Helper;

using MockEventGrid;
using AutoMapper;

var host = new HostBuilder()
    .ConfigureAppConfiguration((context, configBuilder) =>
    {
        configBuilder.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                     .AddEnvironmentVariables();
    })
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        // Stripe settings
        var stripeSettings = configuration.GetSection("Stripe").Get<StripeSettings>();
        if (stripeSettings == null || string.IsNullOrWhiteSpace(stripeSettings.SecretKey))
        {
            throw new InvalidOperationException("Stripe secret key is not configured.");
        }
        StripeConfiguration.ApiKey = stripeSettings.SecretKey;

        services.Configure<StripeSettings>(configuration.GetSection("Stripe"));

        // DbContext
        services.AddDbContext<InnSalesDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // AutoMapper
        services.AddAutoMapper(typeof(PaymentProfile));
        services.AddSingleton<IOrderEventPublisher, OrderEventPublisher>();
        services.AddSingleton<MockEventGridBroker>();
        services.AddScoped<IPaymentTokenService, PaymentTokenService>(); 

        // Services
        services.AddSingleton<IAuthHelper, AuthHelper>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IPaymentTransactionPublisher, PaymentTransactionPublisher>();
        services.AddHostedService<PaymentTimeoutService>();
        services.AddHostedService<PaymentTransactionProcessor>();
 
    })
    .Build();

host.Run();
