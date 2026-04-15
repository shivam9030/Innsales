
using Microsoft.Azure.Functions.Worker;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using OrderMicroservice.Domain;
using OrderMicroservice.Common.DTO;
using OrderMicroservice.Service;
using System.Text.Json;

public class OrderEventBlobFunction
{
    private readonly BlobContainerClient _containerClient;
    private readonly IOrderEventPublisher _publisher;
    public OrderEventBlobFunction(IConfiguration config,IOrderEventPublisher publisher)
    {
        var storageConnection = config["AzureWebJobsStorage"];
        var containerName = config["BlobContainer"] ?? "order-events";
        var blobServiceClient = new BlobServiceClient(storageConnection);
        _containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        _containerClient.CreateIfNotExists();
        _publisher = publisher;
    }

    [Function("OrderEventBlobFunction")]
    public async Task Run(
        [ServiceBusTrigger(
            topicName: "%OrderEventsTopic%",    
        subscriptionName: "%OrderServiceSub%", 
            Connection = "ServiceBusConn")]
        byte[] messageBody)
    {
        var blobPrefix = "orders";
        var now = DateTime.UtcNow;
        var blobName = $"{blobPrefix}/{now:yyyy}/{now:MM}/{now:dd}/{Guid.NewGuid()}.json";
        
        var blobClient = _containerClient.GetBlobClient(blobName);
        await blobClient.UploadAsync(new BinaryData(messageBody), overwrite: true);


        
OrderEvent? dto = null;
        try
        {
            dto = JsonSerializer.Deserialize<OrderEvent>(
                messageBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Function] Deserialization failed: {ex.Message}");
        }

        if (dto is null) return;

        await _publisher.PublishAsync(dto);

        Console.WriteLine($"[Function] Processed order event for OrderId: {dto.OrderId}");
    }
}