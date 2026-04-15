
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using OrderMicroservice.Common.DTO;
using OrderMicroservice.Service;
using System.Text.Json;

namespace OrderMicroservice.FunctionApp;
public class OrderFunctions
{

private readonly IOrderPublisher _publisher;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
               PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

public OrderFunctions( IOrderPublisher publisher)
    {
        _publisher = publisher;
    }

    // GET /v1/orders
    [Function("ListOrders")]
    public async Task<HttpResponseData> ListOrders(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "v1/orders")] HttpRequestData req)
    {
        var res = req.CreateResponse(HttpStatusCode.OK);
        await res.WriteAsJsonAsync(new { message = "ok", items = Array.Empty<object>() });
        return res;
    }

    // POST /v1/orders (idempotent)
    [Function("CreateOrder")]
    public async Task<HttpResponseData> CreateOrder(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "v1/orders")] HttpRequestData req)
    {
        string? idempotencyKey = req.Headers.TryGetValues("Idempotency-Key", out var vals)
            ? vals.FirstOrDefault()
            : null;

        var orderId = Guid.NewGuid().ToString();

        var res = req.CreateResponse(HttpStatusCode.Created);
        res.Headers.Add("Location", $"/v1/orders/{orderId}");
        await res.WriteAsJsonAsync(new { message = "created", orderId, idempotencyKey });
        return res;
    }

    // GET /v1/orders/{orderId}
    [Function("GetOrderById")]
    public async Task<HttpResponseData> GetOrderById(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "v1/orders/{orderId}")] HttpRequestData req,
        string orderId)
    {
        var res = req.CreateResponse(HttpStatusCode.OK);
        await res.WriteAsJsonAsync(new { message = "ok", orderId });
        return res;
    }

    // PATCH /v1/orders/{orderId}
    [Function("PatchOrder")]
    public async Task<HttpResponseData> PatchOrder(
        [HttpTrigger(AuthorizationLevel.Function, "patch", Route = "v1/orders/{orderId}")] HttpRequestData req,
        string orderId)
    {
        var res = req.CreateResponse(HttpStatusCode.OK);
        await res.WriteAsJsonAsync(new { message = "patched", orderId });
        return res;
    }

    // POST /v1/orders/{orderId}/items
    [Function("AddOrderItem")]
    public async Task<HttpResponseData> AddOrderItem(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "v1/orders/{orderId}/items")] HttpRequestData req,
        string orderId)
    {
        var itemId = Guid.NewGuid().ToString();
        var res = req.CreateResponse(HttpStatusCode.Created);
        res.Headers.Add("Location", $"/v1/orders/{orderId}/items/{itemId}");
        await res.WriteAsJsonAsync(new { message = "item added", orderId, itemId });
        return res;
    }

    // PATCH /v1/orders/{orderId}/items/{itemId}
    [Function("UpdateOrderItem")]
    public async Task<HttpResponseData> UpdateOrderItem(
        [HttpTrigger(AuthorizationLevel.Function, "patch", Route = "v1/orders/{orderId}/items/{itemId}")]
        HttpRequestData req,
        string orderId,
        string itemId)
    {
        var res = req.CreateResponse(HttpStatusCode.OK);
        await res.WriteAsJsonAsync(new { message = "item updated", orderId, itemId });
        return res;
    }

    // DELETE /v1/orders/{orderId}/items/{itemId}
    [Function("RemoveOrderItem")]
    public HttpResponseData RemoveOrderItem(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "v1/orders/{orderId}/items/{itemId}")]
        HttpRequestData req,
        string orderId,
        string itemId)
    {
        var res = req.CreateResponse(HttpStatusCode.NoContent);
        return res;
    }

    // POST /v1/orders/{orderId}/cancel
    [Function("CancelOrder")]
    public async Task<HttpResponseData> CancelOrder(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "v1/orders/{orderId}/cancel")] HttpRequestData req,
        string orderId)
    {
        var res = req.CreateResponse(HttpStatusCode.OK);
        await res.WriteAsJsonAsync(new { message = "order canceled", orderId });
        return res;
    }

    // POST /v1/order-events/publish
           [Function("PublishOrderMessage")]
        public async Task<HttpResponseData> PublishOrderEvent(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "v1/order-message/publish")]
            HttpRequestData req)
        {
            var msg = await JsonSerializer.DeserializeAsync<OrderEvent>(req.Body, _jsonOptions);

            if (msg is null ||
                msg.OrderId == Guid.Empty ||
                string.IsNullOrWhiteSpace(msg.CustomerId) ||
                string.IsNullOrWhiteSpace(msg.OrderStatus) ||
                string.IsNullOrWhiteSpace(msg.PaymentStatus))
            {
                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteAsJsonAsync(new { error = "Missing required fields" });
                return bad;
            }

            await _publisher.PublishAsync(msg);

            var res = req.CreateResponse(HttpStatusCode.Accepted);
            await res.WriteAsJsonAsync(new { message = "published", orderId = msg.OrderId });
            return res;
        }
   

}



