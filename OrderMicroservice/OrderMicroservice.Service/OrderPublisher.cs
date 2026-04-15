
// Messaging/OrderPublisher.cs
using System;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using OrderMicroservice.Common.DTO;
using Microsoft.Extensions.Configuration;
namespace OrderMicroservice.Service
{

public class OrderPublisher : IOrderPublisher
{
     private readonly ServiceBusClient _client;
        private readonly ServiceBusSender _sender;
    private static readonly Guid HardcodedVendorId = Guid.Parse("11111111-2222-3333-4444-555555555555");
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

    public OrderPublisher(IConfiguration config)
    {
        var cs = config["ServiceBusConn"]!;
        var topic = config["OrderEventsTopic"] ?? "order-events-topic";
        _client = new ServiceBusClient(cs);
        _sender = _client.CreateSender(topic);
        Console.WriteLine("[SB-PUBLISHER] OrderPublisher initialized successfully");
    }
 
    public async Task PublishAsync(OrderEvent message)
    {
        if (message.VendorId == Guid.Empty)
        throw new InvalidOperationException("VendorId must be provided in request");

        var json = JsonSerializer.Serialize(message, _jsonOptions);
               var sbMessage = new ServiceBusMessage(json)
        {
            ContentType = "application/json",
            MessageId = message.OrderId.ToString()
        };


        await _sender.SendMessageAsync(sbMessage);
        Console.WriteLine($"[SB] Published OrderId={message.OrderId}, VendorId={message.VendorId}");
    }
}
}