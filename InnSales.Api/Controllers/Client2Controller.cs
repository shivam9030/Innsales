
using Microsoft.AspNetCore.Mvc;
using Azure.Storage.Blobs;
using System.Text.Json;
using System.Text;

namespace InnSales.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/client2-events")]
    public sealed class Client2Controller : ControllerBase
    {
        private readonly BlobContainerClient _container;

        public Client2Controller(IConfiguration config)
        {
            var storageConn = config["AzureWebJobsStorage"];
            var containerName = config["SalesEvents:ContainerName"] ?? "sales-events";

            var blobServiceClient = new BlobServiceClient(storageConn);
            _container = blobServiceClient.GetBlobContainerClient(containerName);
            _container.CreateIfNotExists();
        }

        [HttpPost]
        public async Task<IActionResult> Receive([FromBody] JsonElement events)
        {
            Console.WriteLine("InnSales received Sales Order EventGrid event:");

            if (events.ValueKind != JsonValueKind.Array)
                return BadRequest("Invalid Event Grid payload");

            foreach (var ev in events.EnumerateArray())
            {
                var eventType = ev.GetProperty("eventType").GetString();

                if (eventType != "SalesOrderCreated")
                    continue;

                var data = ev.GetProperty("data");

                var vendorId = data.GetProperty("vendorId").GetString();
                var orderId = data.GetProperty("orderId").GetString();

                // Blob path
                var now = DateTime.UtcNow;
                var blobName =
                    $"vendor-{vendorId}/" +
                    $"{now:yyyy}/{now:MM}/{now:dd}/" +
                    $"{orderId}-{Guid.NewGuid()}.json";

                var blobClient = _container.GetBlobClient(blobName);

                //Store full event payload
                var json = JsonSerializer.Serialize(ev, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                await blobClient.UploadAsync(
                    new BinaryData(Encoding.UTF8.GetBytes(json)),
                    overwrite: true
                );

                Console.WriteLine(
                    $"[Webhook Stored] Vendor={vendorId}, OrderId={orderId}, Blob={blobName}"
                );
            }

            return Ok();
        }
    }
}
