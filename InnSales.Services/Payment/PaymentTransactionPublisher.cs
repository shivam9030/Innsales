
// // ServiceBusPaymentPublisher.cs
// using System.Text;
// using System.Text.Json;
// using Azure.Messaging.ServiceBus;
// using Microsoft.Extensions.Configuration;
// using InnSales.Common.DTO;
// namespace InnSales.Services
// {
//     public sealed class PaymentTransactionPublisher : IPaymentTransactionPublisher, IAsyncDisposable
//     {
//         private readonly ServiceBusClient _client;
//         private readonly ServiceBusSender _sender;

//         public  PaymentTransactionPublisher(IConfiguration config)
//         {
//             var cs = config["ServiceBus:ConnectionString"]!;
//             var topic = config["ServiceBus:PaymentTopic"]!;
//             Console.WriteLine($"[SB-PUBLISHER] Initializing with ConnectionString: {cs}, Topic: {topic}");
          
//             _client = new ServiceBusClient(cs);
//             _sender = _client.CreateSender(topic);
//             Console.WriteLine("[SB-PUBLISHER] PaymentTransactionPublisher initialized successfully");
            
//         }

//         public async Task PublishAsync(PaymentTransactionMessage message)
//         {
//             Console.WriteLine($"[SB-PUBLISHER] Publishing message for OrderId: {message.OrderId}, Success: {message.Success}");
           

            
//             var payload = JsonSerializer.Serialize(message);
//             var sbMsg = new ServiceBusMessage(Encoding.UTF8.GetBytes(payload))
//             {
//                 ContentType   = "application/json",
//                 Subject       = message.Success ? "PaymentSucceeded" : "PaymentFailed",
//                 CorrelationId = message.OrderId.ToString(),
//                 MessageId     = message.TransactionId ?? Guid.NewGuid().ToString()
//             };

//             sbMsg.ApplicationProperties["OrderId"] = message.OrderId.ToString();
//             sbMsg.ApplicationProperties["Success"] = message.Success;

//             try
//             {
//                 await _sender.SendMessageAsync(sbMsg);
//                 Console.WriteLine($"[SB-PUBLISHER] Message published successfully. MessageId: {sbMsg.MessageId}, Subject: {sbMsg.Subject}");
             
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"[SB-PUBLISHER] ERROR publishing message for OrderId: {message.OrderId}. Exception: {ex.Message}");
               
//             }
//         }

//         public async ValueTask DisposeAsync()
//         {
//             await _sender.DisposeAsync();
//                        await _client.DisposeAsync();
//         }
//     }
// }


// ServiceBusPaymentPublisher.cs
using System.Text;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using InnSales.Common.DTO;

namespace InnSales.Services
{
    public sealed class PaymentTransactionPublisher : IPaymentTransactionPublisher, IAsyncDisposable
    {
        private readonly ServiceBusClient _client;
        private readonly ServiceBusSender _sender;
        private readonly IServiceProvider _serviceProvider;

        public PaymentTransactionPublisher(IConfiguration config, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            var cs = config["ServiceBus:ConnectionString"]!;
            var topic = config["ServiceBus:PaymentTopic"]!;
            Console.WriteLine($"[SB-PUBLISHER] Initializing with ConnectionString: {cs}, Topic: {topic}");
          
            _client = new ServiceBusClient(cs);
            _sender = _client.CreateSender(topic);
            Console.WriteLine("[SB-PUBLISHER] PaymentTransactionPublisher initialized successfully");
        }

        public async Task PublishAsync(PaymentTransactionMessage message)
        {
            Console.WriteLine($"[SB-PUBLISHER] Publishing message for OrderId: {message.OrderId}, Success: {message.Success}");
            
            var payload = JsonSerializer.Serialize(message);
            var sbMsg = new ServiceBusMessage(Encoding.UTF8.GetBytes(payload))
            {
                ContentType   = "application/json",
                Subject       = message.Success ? "PaymentSucceeded" : "PaymentFailed",
                CorrelationId = message.OrderId.ToString(),
                MessageId     = message.TransactionId ?? Guid.NewGuid().ToString()
            };

            sbMsg.ApplicationProperties["OrderId"] = message.OrderId.ToString();
            sbMsg.ApplicationProperties["Success"] = message.Success;

            try
            {
                await _sender.SendMessageAsync(sbMsg);
                Console.WriteLine($"[SB-PUBLISHER] Message published successfully. MessageId: {sbMsg.MessageId}, Subject: {sbMsg.Subject}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SB-PUBLISHER] ERROR publishing message for OrderId: {message.OrderId}. Exception: {ex.Message}");
                Console.WriteLine($"[SB-PUBLISHER] FALLBACK: Processing payment transaction synchronously bypassing Service Bus.");
                
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
                    await orderService.HandlePaymentTransactionAsync(message);
                    Console.WriteLine($"[SB-PUBLISHER] FALLBACK SUCCESS: Payment transaction recorded synchronously for OrderId: {message.OrderId}.");
                }
                catch (Exception fallbackEx)
                {
                    Console.WriteLine($"[SB-PUBLISHER] FALLBACK ERROR: Failed to process transaction synchronously. Exception: {fallbackEx.Message}");
                    // Still throw here if we completely failed to record the payment!
                    throw new Exception("Critical failure: Could not record payment transaction via Service Bus OR synchronous fallback.", fallbackEx);
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            await _sender.DisposeAsync();
            await _client.DisposeAsync();
        }
    }
}