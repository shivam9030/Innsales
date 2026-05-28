using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using InnSales.Common.DTO;

namespace InnSales.Services
{
    public sealed class PaymentTransactionProcessor : BackgroundService
    {
        private readonly ServiceBusProcessor _processor;
        private readonly IServiceProvider _services;

        public PaymentTransactionProcessor(
            IConfiguration config,
            IServiceProvider services)
        {
            _services = services;

            var cs    = config["ServiceBus:ConnectionString"];
            var topic = config["ServiceBus:PaymentTopic"];
            var sub   = config["ServiceBus:OrderSubscription"];

            if (string.IsNullOrWhiteSpace(cs) || cs.Contains("UseDevelopmentEmulator"))
            {
                Console.WriteLine("[SB-PROCESSOR] Service Bus connection string is invalid or uses emulator. Disabling processor.");
                return;
            }

            var client = new ServiceBusClient(cs);

            _processor = client.CreateProcessor(topic, sub, new ServiceBusProcessorOptions
            {
                MaxConcurrentCalls   = 4,
                AutoCompleteMessages = false
            });

            _processor.ProcessMessageAsync += OnMessageAsync;
            _processor.ProcessErrorAsync   += OnErrorAsync;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_processor == null) return Task.CompletedTask;
            return _processor.StartProcessingAsync(stoppingToken);
        }

        private async Task OnMessageAsync(ProcessMessageEventArgs args)
        {
            try
            {
                var body = args.Message.Body.ToString();
                Console.WriteLine($"[SB-PROCESSOR] Message body: {body}");

                var msg = JsonSerializer.Deserialize<PaymentTransactionMessage>(
                    body,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (msg == null)
                {
                    Console.WriteLine("[SB-PROCESSOR] Invalid message. Skipping.");
                    await args.CompleteMessageAsync(args.Message);
                    return;
                }

                // Ignore checkout / non-payment events
                if (!msg.Success && msg.TransactionId == null)
                {
                    Console.WriteLine(
                        $"[SB-PROCESSOR] Ignoring non-payment message. OrderId={msg.OrderId}"
                    );

                    await args.CompleteMessageAsync(args.Message);
                    return;
                }

                using var scope = _services.CreateScope();
                var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();

                await orderService.HandlePaymentTransactionAsync(msg);

                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SB-PROCESSOR] ERROR: {ex.Message}");
                await args.AbandonMessageAsync(args.Message);
            }
        }

        private Task OnErrorAsync(ProcessErrorEventArgs args)
        {
            Console.WriteLine($"[SB-PROCESSOR] ERROR: {args.Exception?.Message}");
            return Task.CompletedTask;
        }

        public override async Task StopAsync(CancellationToken ct)
        {
            await _processor.StopProcessingAsync(ct);
            await _processor.DisposeAsync();
        }
    }
}
