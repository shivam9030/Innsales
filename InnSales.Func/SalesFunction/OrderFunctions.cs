

// using System;
// using System.Net;
// using System.Threading.Tasks;
// using System.IdentityModel.Tokens.Jwt;
// using Microsoft.Azure.Functions.Worker;
// using Microsoft.Azure.Functions.Worker.Http;
// using Microsoft.Extensions.Logging;
// using Microsoft.Extensions.Configuration;
// using System.Security.Claims;
// using InnSales.Services;
// using InnSales.Common.DTO;
// using Helper; // IAuthHelper

// namespace InnSales.Functions
// {
//     public class OrderFunctions
//     {
//         private readonly ILogger<OrderFunctions> _logger;
//         private readonly IConfiguration _config;
//         private readonly IAuthHelper _auth;
//         private readonly IOrderService _orderService;
//         private readonly IBasketService _basketService;

//         public OrderFunctions(
//             ILogger<OrderFunctions> logger,
//             IConfiguration config,
//             IAuthHelper auth,
//             IOrderService orderService,
//             IBasketService basketService)
//         {
//             _logger = logger;
//             _config = config;
//             _auth = auth;
//             _orderService = orderService;
//             _basketService = basketService;
//         }

//         // GET api/v1/orders/{id}  (Anonymous)
//         [Function("GetOrderById")]
//         public async Task<HttpResponseData> GetOrderById(
//             [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/orders/{id:guid}")]
//             HttpRequestData req,
//             Guid id)
//         {
//             try
//             {
//                 var order = await _orderService.GetOrderByIdAsync(id);
//                 var res = req.CreateResponse(order is null ? HttpStatusCode.NotFound : HttpStatusCode.OK);
//                 if (order is null)
//                 {
//                     await res.WriteStringAsync("Order not found.");
//                 }
//                 else
//                 {
//                     await res.WriteAsJsonAsync(order);
//                 }
//                 return res;
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error getting order by id {OrderId}", id);
//                 var err = req.CreateResponse(HttpStatusCode.InternalServerError);
//                 await err.WriteStringAsync($"Error: {ex.Message}");
//                 return err;
//             }
//         }

//         // GET api/v1/orders  (Anonymous)
//         [Function("GetAllOrders")]
//         public async Task<HttpResponseData> GetAllOrders(
//             [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/orders")]
//             HttpRequestData req)
//         {
//             try
//             {var userId = GetUserIdFromBearer(req);
//                 var orders = await _orderService.GetAllOrdersAsync(userId);            
//                 var res = req.CreateResponse(HttpStatusCode.OK);
//                 await res.WriteAsJsonAsync(orders);
//                 return res;
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error getting all orders");
//                 var err = req.CreateResponse(HttpStatusCode.InternalServerError);
//                 await err.WriteStringAsync($"Error: {ex.Message}");
//                 return err;
//             }
//         }

//         // DELETE api/v1/orders/{id}  (Authorize)
//         [Function("DeleteOrder")]
//         public async Task<HttpResponseData> DeleteOrder(
//             [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "v1/orders/{id:guid}")]
//             HttpRequestData req,
//             Guid id)
//         {
//             if (!_auth.IsAuthenticated(req)) return _auth.Unauthorized(req);

//             try
//             {
//                 await _orderService.DeleteOrderAsync(id);
//                 return req.CreateResponse(HttpStatusCode.NoContent);
//             }
//             catch (KeyNotFoundException)
//             {
//                 var notFound = req.CreateResponse(HttpStatusCode.NotFound);
//                 await notFound.WriteStringAsync("Order not found.");
//                 return notFound;
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error deleting order {OrderId}", id);
//                 var err = req.CreateResponse(HttpStatusCode.InternalServerError);
//                 await err.WriteStringAsync($"Error: {ex.Message}");
//                 return err;
//             }
//         }

//         // POST api/v1/orders/create?userId=...  (Authorize)
//         [Function("CreateOrderFromBasket")]
//         public async Task<HttpResponseData> CreateOrderFromBasket(
//             [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/orders/create")]
//             HttpRequestData req)
//         {
//             if (!_auth.IsAuthenticated(req)) return _auth.Unauthorized(req);

//             // Controller expects userId in query string, so mirror that:
//             var userId = GetUserIdFromBearer(req);
//             if (string.IsNullOrWhiteSpace(userId))
//             {
//                 var bad = req.CreateResponse(HttpStatusCode.BadRequest);
//                 await bad.WriteStringAsync("Query parameter 'userId' is required.");
//                 return bad;
//             }

//             try
//             {
//                 var basket = await _basketService.GetBasketAsync(userId);
//                 var order  = await _orderService.CreateOrderFromBasketAsync(userId, basket);

//                 var created = req.CreateResponse(HttpStatusCode.Created);
//                 var location = new Uri($"{req.Url.GetLeftPart(UriPartial.Authority)}/api/v1/orders/{order.Id}");
//                 created.Headers.Add("Location", location.ToString());
//                 await created.WriteAsJsonAsync(order);
//                 return created;
//             }
//             catch (KeyNotFoundException knf)
//             {
//                 var notFound = req.CreateResponse(HttpStatusCode.NotFound);
//                 await notFound.WriteStringAsync(knf.Message);
//                 return notFound;
//             }
//             catch (InvalidOperationException ioe)
//             {
//                 var bad = req.CreateResponse(HttpStatusCode.BadRequest);
//                 await bad.WriteStringAsync(ioe.Message);
//                 return bad;
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error creating order from basket for user {UserId}", userId);
//                 var err = req.CreateResponse(HttpStatusCode.InternalServerError);
//                 await err.WriteStringAsync($"Error: {ex.Message}");
//                 return err;
//             }
//         }

//         // POST api/v1/orders/cancel/{orderId}  (Authorize)
//         [Function("CancelOrder")]
//         public async Task<HttpResponseData> CancelOrder(
//             [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/orders/cancel/{orderId:guid}")]
//             HttpRequestData req,
//             Guid orderId)
//         {
//             if (!_auth.IsAuthenticated(req)) return _auth.Unauthorized(req);

//             try
//             {
//                 await _orderService.CancelOrderAsync(orderId);
//                 var ok = req.CreateResponse(HttpStatusCode.OK);
//                 await ok.WriteStringAsync("Order cancelled and inventory restocked.");
//                 return ok;
//             }
//             catch (KeyNotFoundException)
//             {
//                 var notFound = req.CreateResponse(HttpStatusCode.NotFound);
//                 await notFound.WriteStringAsync("Order not found.");
//                 return notFound;
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error cancelling order {OrderId}", orderId);
//                 var err = req.CreateResponse(HttpStatusCode.InternalServerError);
//                 await err.WriteStringAsync($"Error: {ex.Message}");
//                 return err;
//             }
//         }

//         // POST api/v1/orders/handle-failed-payment/{orderId}  (Authorize)
//         [Function("HandleFailedPayment")]
//         public async Task<HttpResponseData> HandleFailedPayment(
//             [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/orders/handle-failed-payment/{orderId:guid}")]
//             HttpRequestData req,
//             Guid orderId)
//         {
//             if (!_auth.IsAuthenticated(req)) return _auth.Unauthorized(req);

//             try
//             {
//                 await _orderService.HandleFailedPaymentAsync(orderId);
//                 var ok = req.CreateResponse(HttpStatusCode.OK);
//                 await ok.WriteStringAsync("Inventory restocked due to failed payment.");
//                 return ok;
//             }
//             catch (KeyNotFoundException)
//             {
//                 var notFound = req.CreateResponse(HttpStatusCode.NotFound);
//                 await notFound.WriteStringAsync("Order not found.");
//                 return notFound;
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error handling failed payment for order {OrderId}", orderId);
//                 var err = req.CreateResponse(HttpStatusCode.InternalServerError);
//                 await err.WriteStringAsync($"Error: {ex.Message}");
//                 return err;
//             }
//         }

//         // // ---- helpers ----
//         // private static string? GetQuery(HttpRequestData req, string key)
//         // {
//         //     var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
//         //     return query.Get(key);
//         // }
//         private static string? GetUserIdFromBearer(HttpRequestData req)
//         {
//             if (!req.Headers.TryGetValues("Authorization", out var vals)) return null;
//             var raw = vals.FirstOrDefault();
//             if (string.IsNullOrWhiteSpace(raw) || !raw.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) return null;

//             var token = raw["Bearer ".Length..].Trim();
//             if (string.IsNullOrWhiteSpace(token)) return null;

//             try
//             {
//                 var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
//                 var claims = jwt.Claims.ToList();

//                 return
//                     claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ??
//                     claims.FirstOrDefault(c => c.Type.Equals("nameid", StringComparison.OrdinalIgnoreCase))?.Value ??
//                     claims.FirstOrDefault(c => c.Type.Equals("sub", StringComparison.OrdinalIgnoreCase))?.Value;
//             }
//             catch { return null; }
//         }

//         // Tries to get UserId from the returned order/DTO without knowing exact type at compile time.
//         // Assumes a property named "UserId" exists (string); adapt if your model uses something else.
//         private static string? TryGetOrderUserId(object order)
//         {
//             var prop = order.GetType().GetProperty("UserId");
//             if (prop is null) return null;
//             var val = prop.GetValue(order) as string;
//             return val;
//         }
//     }
// }
using System;
using System.Net;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using InnSales.Services;
using InnSales.Common.DTO;
using Helper;

namespace InnSales.Functions
{
    public class OrderFunctions
    {
        private readonly ILogger<OrderFunctions> _logger;
        private readonly IConfiguration _config;
        private readonly IAuthHelper _auth;
        private readonly IOrderService _orderService;
        private readonly IBasketService _basketService;

        public OrderFunctions(
            ILogger<OrderFunctions> logger,
            IConfiguration config,
            IAuthHelper auth,
            IOrderService orderService,
            IBasketService basketService)
        {
            _logger = logger;
            _config = config;
            _auth = auth;
            _orderService = orderService;
            _basketService = basketService;
        }

        // GET api/v1/orders/{id}
        [Function("GetOrderById")]
        public async Task<HttpResponseData> GetOrderById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/orders/{id:guid}")]
            HttpRequestData req,
            Guid id)
        {
            _logger.LogInformation("GetOrderById triggered for OrderId {OrderId}", id);

            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);
                var res = req.CreateResponse(order is null ? HttpStatusCode.NotFound : HttpStatusCode.OK);

                if (order is null)
                {
                    _logger.LogWarning("Order {OrderId} not found", id);
                    await res.WriteStringAsync("Order not found.");
                }
                else
                {
                    _logger.LogInformation("Order {OrderId} retrieved successfully", id);
                    await res.WriteAsJsonAsync(order);
                }

                return res;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order by id {OrderId}", id);
                var err = req.CreateResponse(HttpStatusCode.InternalServerError);
                await err.WriteStringAsync($"Error: {ex.Message}");
                return err;
            }
        }

        // GET api/v1/orders
        [Function("GetAllOrders")]
        public async Task<HttpResponseData> GetAllOrders(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/orders")]
            HttpRequestData req)
        {
            var userId = GetUserIdFromBearer(req);
            _logger.LogInformation("GetAllOrders triggered for UserId {UserId}", userId);

            try
            {
                var orders = await _orderService.GetAllOrdersAsync(userId);
                var res = req.CreateResponse(HttpStatusCode.OK);

                _logger.LogInformation("Retrieved orders for UserId {UserId}", userId);
                await res.WriteAsJsonAsync(orders);
                return res;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all orders for UserId {UserId}", userId);
                var err = req.CreateResponse(HttpStatusCode.InternalServerError);
                await err.WriteStringAsync($"Error: {ex.Message}");
                return err;
            }
        }

        // DELETE api/v1/orders/{id}
        [Function("DeleteOrder")]
        public async Task<HttpResponseData> DeleteOrder(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "v1/orders/{id:guid}")]
            HttpRequestData req,
            Guid id)
        {
            _logger.LogInformation("DeleteOrder triggered for OrderId {OrderId}", id);

            if (!_auth.IsAuthenticated(req))
            {
                _logger.LogWarning("Unauthorized delete attempt for OrderId {OrderId}", id);
                return _auth.Unauthorized(req);
            }

            try
            {
                await _orderService.DeleteOrderAsync(id);
                _logger.LogInformation("Order {OrderId} deleted successfully", id);
                return req.CreateResponse(HttpStatusCode.NoContent);
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Order {OrderId} not found for deletion", id);
                var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                await notFound.WriteStringAsync("Order not found.");
                return notFound;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting order {OrderId}", id);
                var err = req.CreateResponse(HttpStatusCode.InternalServerError);
                await err.WriteStringAsync($"Error: {ex.Message}");
                return err;
            }
        }

        // POST api/v1/orders/create
        [Function("CreateOrderFromBasket")]
        public async Task<HttpResponseData> CreateOrderFromBasket(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/orders/create")]
            HttpRequestData req)
        {
            _logger.LogInformation("CreateOrderFromBasket triggered");

            if (!_auth.IsAuthenticated(req))
            {
                _logger.LogWarning("Unauthorized order creation attempt");
                return _auth.Unauthorized(req);
            }

            var userId = GetUserIdFromBearer(req);
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("Order creation failed: missing UserId in token");
                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteStringAsync("Query parameter 'userId' is required.");
                return bad;
            }

            try
            {
                _logger.LogInformation("Creating order from basket for UserId {UserId}", userId);

                var basket = await _basketService.GetBasketAsync(userId);
                var order = await _orderService.CreateOrderFromBasketAsync(userId, basket);

                var created = req.CreateResponse(HttpStatusCode.Created);
                var location = new Uri($"{req.Url.GetLeftPart(UriPartial.Authority)}/api/v1/orders/{order.Id}");
                created.Headers.Add("Location", location.ToString());

                _logger.LogInformation("Order {OrderId} created successfully for UserId {UserId}", order.Id, userId);
                await created.WriteAsJsonAsync(order);
                return created;
            }
            catch (KeyNotFoundException knf)
            {
                _logger.LogWarning(knf, "Basket or user not found for UserId {UserId}", userId);
                var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                await notFound.WriteStringAsync(knf.Message);
                return notFound;
            }
            catch (InvalidOperationException ioe)
            {
                _logger.LogWarning(ioe, "Invalid operation creating order for UserId {UserId}", userId);
                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteStringAsync(ioe.Message);
                return bad;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order from basket for UserId {UserId}", userId);
                var err = req.CreateResponse(HttpStatusCode.InternalServerError);
                await err.WriteStringAsync($"Error: {ex.Message}");
                return err;
            }
        }

        // POST api/v1/orders/cancel/{orderId}
        [Function("CancelOrder")]
        public async Task<HttpResponseData> CancelOrder(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/orders/cancel/{orderId:guid}")]
            HttpRequestData req,
            Guid orderId)
        {
            _logger.LogInformation("CancelOrder triggered for OrderId {OrderId}", orderId);

            if (!_auth.IsAuthenticated(req))
            {
                _logger.LogWarning("Unauthorized cancel attempt for OrderId {OrderId}", orderId);
                return _auth.Unauthorized(req);
            }

            try
            {
                await _orderService.CancelOrderAsync(orderId);
                _logger.LogInformation("Order {OrderId} cancelled successfully", orderId);

                var ok = req.CreateResponse(HttpStatusCode.OK);
                await ok.WriteStringAsync("Order cancelled and inventory restocked.");
                return ok;
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Order {OrderId} not found for cancellation", orderId);
                var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                await notFound.WriteStringAsync("Order not found.");
                return notFound;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order {OrderId}", orderId);
                var err = req.CreateResponse(HttpStatusCode.InternalServerError);
                await err.WriteStringAsync($"Error: {ex.Message}");
                return err;
            }
        }

        // POST api/v1/orders/handle-failed-payment/{orderId}
        [Function("HandleFailedPayment")]
        public async Task<HttpResponseData> HandleFailedPayment(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/orders/handle-failed-payment/{orderId:guid}")]
            HttpRequestData req,
            Guid orderId)
        {
            _logger.LogInformation("HandleFailedPayment triggered for OrderId {OrderId}", orderId);

            if (!_auth.IsAuthenticated(req))
            {
                _logger.LogWarning("Unauthorized failed-payment handling attempt for OrderId {OrderId}", orderId);
                return _auth.Unauthorized(req);
            }

            try
            {
                await _orderService.HandleFailedPaymentAsync(orderId);
                _logger.LogInformation("Failed payment handled for OrderId {OrderId}", orderId);

                var ok = req.CreateResponse(HttpStatusCode.OK);
                await ok.WriteStringAsync("Inventory restocked due to failed payment.");
                return ok;
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Order {OrderId} not found during failed payment handling", orderId);
                var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                await notFound.WriteStringAsync("Order not found.");
                return notFound;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling failed payment for OrderId {OrderId}", orderId);
                var err = req.CreateResponse(HttpStatusCode.InternalServerError);
                await err.WriteStringAsync($"Error: {ex.Message}");
                return err;
            }
        }

        // ---- helpers ----
        private static string? GetUserIdFromBearer(HttpRequestData req)
        {
            if (!req.Headers.TryGetValues("Authorization", out var vals)) return null;
            var raw = vals.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(raw) || !raw.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) return null;

            var token = raw["Bearer ".Length..].Trim();
            if (string.IsNullOrWhiteSpace(token)) return null;

            try
            {
                var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
                var claims = jwt.Claims.ToList();

                return
                    claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ??
                    claims.FirstOrDefault(c => c.Type.Equals("nameid", StringComparison.OrdinalIgnoreCase))?.Value ??
                    claims.FirstOrDefault(c => c.Type.Equals("sub", StringComparison.OrdinalIgnoreCase))?.Value;
            }
            catch
            {
                return null;
            }
        }
    }
}
