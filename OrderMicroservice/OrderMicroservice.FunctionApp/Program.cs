
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using Azure.Core.Serialization;
using Microsoft.Azure.Functions.Worker;
using OrderMicroservice.Service;
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


        services.Configure<WorkerOptions>(options =>
        {
            options.Serializer = new JsonObjectSerializer(new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
                WriteIndented = false
            });
        });

        // Use Event Grid (mock) publisher for order events
        services.AddSingleton(new MockEventGridBroker("UseDevelopmentStorage=true"));
        services.AddScoped<IOrderEventPublisher, EventGridOrderPublisher>();
        services.AddScoped<IOrderPublisher, OrderPublisher>();
        services.AddSingleton<IEventGridProvisioner, MockEventGridProvisioner>();
       services.AddScoped<IVendorSubscriptionService, VendorSubscriptionService>();


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
