

using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Helper;               // IAuthHelper
using InnSales.Services;     // IBasketService
using InnSales.Common.DTO;   // AddBasketItemDto

// namespace InnSales.Functions
// {
//     public class BasketFunctions
//     {
//         private readonly ILogger<BasketFunctions> _logger;
//         private readonly IConfiguration _config;
//         private readonly IServiceScopeFactory _scopeFactory;
//         private readonly IAuthHelper _auth;

//         public BasketFunctions(
//             IServiceScopeFactory scopeFactory,
//             ILogger<BasketFunctions> logger,
//             IConfiguration config,
//             IAuthHelper auth)
//         {
//             _scopeFactory = scopeFactory;
//             _logger = logger;
//             _config = config;
//             _auth = auth;
//         }

//         // GET v1/basket
//         [Function("GetBasket")]
//         public async Task<HttpResponseData> GetBasket(
//             [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/basket")] HttpRequestData req)
//         {
//             if (!_auth.IsAuthenticated(req)) return _auth.Unauthorized(req);

//             var userId = GetUserIdFromBearer(req);
//             if (string.IsNullOrEmpty(userId)) return _auth.Unauthorized(req);

//             using var scope = _scopeFactory.CreateScope();
//             var basketService = scope.ServiceProvider.GetRequiredService<IBasketService>();

//             var items = await basketService.GetBasketAsync(userId);
//             var res = req.CreateResponse(HttpStatusCode.OK);
//             await res.WriteAsJsonAsync(items);
//             return res;
//         }

//         // POST v1/basket/add
//         [Function("AddToBasket")]
//         public async Task<HttpResponseData> AddToBasket(
//             [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/basket/add")] HttpRequestData req)
//         {
//             if (!_auth.IsAuthenticated(req)) return _auth.Unauthorized(req);

//             var dto = await req.ReadFromJsonAsync<AddBasketItemDto>() ?? new AddBasketItemDto();
//             var userId = GetUserIdFromBearer(req);
//             if (string.IsNullOrEmpty(userId)) return _auth.Unauthorized(req);

//             dto.UserId = userId;

//             using var scope = _scopeFactory.CreateScope();
//             var basketService = scope.ServiceProvider.GetRequiredService<IBasketService>();

//             await basketService.AddToBasketAsync(dto);

//             var res = req.CreateResponse(HttpStatusCode.OK);
//             await res.WriteAsJsonAsync(new { message = "Item added to basket." });
//             return res;
//         }

//         // PUT v1/basket/update/{id}
//         [Function("UpdateBasketQuantity")]
//         public async Task<HttpResponseData> UpdateQuantity(
//             [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "v1/basket/update/{id:guid}")] HttpRequestData req,
//             Guid id)
//         {
//             if (!_auth.IsAuthenticated(req)) return _auth.Unauthorized(req);

//             var quantity = await req.ReadFromJsonAsync<int>();

//             using var scope = _scopeFactory.CreateScope();
//             var basketService = scope.ServiceProvider.GetRequiredService<IBasketService>();

//             try
//             {
//                 await basketService.UpdateQuantityAsync(id, quantity);
//                 var res = req.CreateResponse(HttpStatusCode.OK);
//                 await res.WriteAsJsonAsync(new { success = true, updatedQuantity = quantity });
//                 return res;
//             }
//             catch (Exception ex)
//             {
//                 var bad = req.CreateResponse(HttpStatusCode.BadRequest);
//                 await bad.WriteAsJsonAsync(new { error = ex.Message });
//                 return bad;
//             }
//         }

//         // DELETE v1/basket/remove/{id}
//         [Function("RemoveBasketItem")]
//         public async Task<HttpResponseData> RemoveItem(
//             [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "v1/basket/remove/{id:guid}")] HttpRequestData req,
//             Guid id)
//         {
//             if (!_auth.IsAuthenticated(req)) return _auth.Unauthorized(req);

//             using var scope = _scopeFactory.CreateScope();
//             var basketService = scope.ServiceProvider.GetRequiredService<IBasketService>();

//             await basketService.RemoveItemAsync(id);

//             var res = req.CreateResponse(HttpStatusCode.OK);
//             await res.WriteAsJsonAsync(new { message = "Item removed from basket." });
//             return res;
//         }

//         // POST v1/basket/checkout
//         [Function("BasketCheckout")]
//         public async Task<HttpResponseData> Checkout(
//             [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/basket/checkout")] HttpRequestData req)
//         {
//             if (!_auth.IsAuthenticated(req)) return _auth.Unauthorized(req);

//             var userId = GetUserIdFromBearer(req);
//             if (string.IsNullOrEmpty(userId)) return _auth.Unauthorized(req);

//             using var scope = _scopeFactory.CreateScope();
//             var basketService = scope.ServiceProvider.GetRequiredService<IBasketService>();

//             try
//             {
//                 var order = await basketService.CheckoutAsync(userId);
//                 var res = req.CreateResponse(HttpStatusCode.OK);
//                 await res.WriteAsJsonAsync(order);
//                 return res;
//             }
//             catch (Exception ex)
//             {
//                 var bad = req.CreateResponse(HttpStatusCode.BadRequest);
//                 await bad.WriteAsJsonAsync(new { message = "Checkout failed, please try again.", error = ex.Message });
//                 return bad;
//             }
//         }

//         // POST v1/basket/apply-promo
//         [Function("ApplyBasketPromo")]
//         public async Task<HttpResponseData> ApplyPromo(
//             [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/basket/apply-promo")] HttpRequestData req)
//         {
//             if (!_auth.IsAuthenticated(req)) return _auth.Unauthorized(req);

//             var promoCode = await req.ReadFromJsonAsync<string>();
//             var userId = GetUserIdFromBearer(req);
//             if (string.IsNullOrEmpty(userId)) return _auth.Unauthorized(req);

//             using var scope = _scopeFactory.CreateScope();
//             var basketService = scope.ServiceProvider.GetRequiredService<IBasketService>();

//             try
//             {
//                 await basketService.ApplyPromoCodeAsync(userId, promoCode ?? string.Empty);
//                 var res = req.CreateResponse(HttpStatusCode.OK);
//                 await res.WriteAsJsonAsync(new { message = "Promo code applied successfully." });
//                 return res;
//             }
//             catch (Exception ex)
//             {
//                 var bad = req.CreateResponse(HttpStatusCode.BadRequest);
//                 await bad.WriteAsJsonAsync(new { error = ex.Message });
//                 return bad;
//             }
//         }

//         // --- helper: extract userId from JWT ---
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
//     }
// }

// using System;
// using System.Linq;
// using System.Net;
// using System.Threading.Tasks;
// using System.IdentityModel.Tokens.Jwt;          // if not already present in the file/project
// using System.Security.Claims;                   // if not already present
// using Microsoft.Azure.Functions.Worker;
// using Microsoft.Azure.Functions.Worker.Http;
// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Logging;
// using Helper;

namespace InnSales.Functions
{
    public class BasketFunctions
    {
        private readonly ILogger<BasketFunctions> _logger;
        private readonly IConfiguration _config;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IAuthHelper _auth;

        public BasketFunctions(
            IServiceScopeFactory scopeFactory,
            ILogger<BasketFunctions> logger,
            IConfiguration config,
            IAuthHelper auth)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _config = config;
            _auth = auth;
        }

        // GET v1/basket
        [Function("GetBasket")]
        public async Task<HttpResponseData> GetBasket(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/basket")] HttpRequestData req)
        {
            _logger.LogInformation("GetBasket triggered");

            if (!_auth.IsAuthenticated(req))
            {
                _logger.LogWarning("GetBasket: Unauthorized request");
                return _auth.Unauthorized(req);
            }

            var userId = GetUserIdFromBearer(req);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("GetBasket: UserId extraction failed");
                return _auth.Unauthorized(req);
            }

            using var scope = _scopeFactory.CreateScope();
            var basketService = scope.ServiceProvider.GetRequiredService<IBasketService>();

            var items = await basketService.GetBasketAsync(userId);
            _logger.LogInformation("GetBasket: Basket fetched for UserId={UserId}", userId);

            var res = req.CreateResponse(HttpStatusCode.OK);
            await res.WriteAsJsonAsync(items);
            return res;
        }

        // POST v1/basket/add
        [Function("AddToBasket")]
        public async Task<HttpResponseData> AddToBasket(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/basket/add")] HttpRequestData req)
        {
            _logger.LogInformation("AddToBasket triggered");

            if (!_auth.IsAuthenticated(req))
            {
                _logger.LogWarning("AddToBasket: Unauthorized request");
                return _auth.Unauthorized(req);
            }

            var dto = await req.ReadFromJsonAsync<AddBasketItemDto>() ?? new AddBasketItemDto();
            var userId = GetUserIdFromBearer(req);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("AddToBasket: UserId extraction failed");
                return _auth.Unauthorized(req);
            }

            dto.UserId = userId;

            using var scope = _scopeFactory.CreateScope();
            var basketService = scope.ServiceProvider.GetRequiredService<IBasketService>();

            await basketService.AddToBasketAsync(dto);
            _logger.LogInformation("AddToBasket: Item added for UserId={UserId}, ProductId={ProductId}", userId, dto.ProductId);

            var res = req.CreateResponse(HttpStatusCode.OK);
            await res.WriteAsJsonAsync(new { message = "Item added to basket." });
            return res;
        }

        // PUT v1/basket/update/{id}
        [Function("UpdateBasketQuantity")]
        public async Task<HttpResponseData> UpdateQuantity(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "v1/basket/update/{id:guid}")] HttpRequestData req,
            Guid id)
        {
            _logger.LogInformation("UpdateBasketQuantity triggered for BasketItemId={BasketItemId}", id);

            if (!_auth.IsAuthenticated(req))
            {
                _logger.LogWarning("UpdateBasketQuantity: Unauthorized request for BasketItemId={BasketItemId}", id);
                return _auth.Unauthorized(req);
            }

            var quantity = await req.ReadFromJsonAsync<int>();

            using var scope = _scopeFactory.CreateScope();
            var basketService = scope.ServiceProvider.GetRequiredService<IBasketService>();

            try
            {
                await basketService.UpdateQuantityAsync(id, quantity);
                _logger.LogInformation("UpdateBasketQuantity: Updated BasketItemId={BasketItemId} to quantity={Quantity}", id, quantity);

                var res = req.CreateResponse(HttpStatusCode.OK);
                await res.WriteAsJsonAsync(new { success = true, updatedQuantity = quantity });
                return res;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateBasketQuantity: Failed to update BasketItemId={BasketItemId}", id);

                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteAsJsonAsync(new { error = ex.Message });
                return bad;
            }
        }

        // DELETE v1/basket/remove/{id}
        [Function("RemoveBasketItem")]
        public async Task<HttpResponseData> RemoveItem(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "v1/basket/remove/{id:guid}")] HttpRequestData req,
            Guid id)
        {
            _logger.LogInformation("RemoveBasketItem triggered for BasketItemId={BasketItemId}", id);

            if (!_auth.IsAuthenticated(req))
            {
                _logger.LogWarning("RemoveBasketItem: Unauthorized request for BasketItemId={BasketItemId}", id);
                return _auth.Unauthorized(req);
            }

            using var scope = _scopeFactory.CreateScope();
            var basketService = scope.ServiceProvider.GetRequiredService<IBasketService>();

            await basketService.RemoveItemAsync(id);
            _logger.LogInformation("RemoveBasketItem: Removed BasketItemId={BasketItemId}", id);

            var res = req.CreateResponse(HttpStatusCode.OK);
            await res.WriteAsJsonAsync(new { message = "Item removed from basket." });
            return res;
        }

        // POST v1/basket/checkout
        [Function("BasketCheckout")]
        public async Task<HttpResponseData> Checkout(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/basket/checkout")] HttpRequestData req)
        {
            _logger.LogInformation("BasketCheckout triggered");

            if (!_auth.IsAuthenticated(req))
            {
                _logger.LogWarning("BasketCheckout: Unauthorized request");
                return _auth.Unauthorized(req);
            }

            var userId = GetUserIdFromBearer(req);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("BasketCheckout: UserId extraction failed");
                return _auth.Unauthorized(req);
            }

            using var scope = _scopeFactory.CreateScope();
            var basketService = scope.ServiceProvider.GetRequiredService<IBasketService>();

            try
            {
                var order = await basketService.CheckoutAsync(userId);
                _logger.LogInformation("BasketCheckout: Checkout successful for UserId={UserId}, OrderId={OrderId}", userId, order?.Id);

                var res = req.CreateResponse(HttpStatusCode.OK);
                await res.WriteAsJsonAsync(order);
                return res;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BasketCheckout: Checkout failed for UserId={UserId}", userId);

                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteAsJsonAsync(new { message = "Checkout failed, please try again.", error = ex.Message });
                return bad;
            }
        }

        // POST v1/basket/apply-promo
        [Function("ApplyBasketPromo")]
        public async Task<HttpResponseData> ApplyPromo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/basket/apply-promo")] HttpRequestData req)
        {
            _logger.LogInformation("ApplyBasketPromo triggered");

            if (!_auth.IsAuthenticated(req))
            {
                _logger.LogWarning("ApplyBasketPromo: Unauthorized request");
                return _auth.Unauthorized(req);
            }

            var promoCode = await req.ReadFromJsonAsync<string>();

            var userId = GetUserIdFromBearer(req);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("ApplyBasketPromo: UserId extraction failed");
                return _auth.Unauthorized(req);
            }

            using var scope = _scopeFactory.CreateScope();
            var basketService = scope.ServiceProvider.GetRequiredService<IBasketService>();

            try
            {
                await basketService.ApplyPromoCodeAsync(userId, promoCode ?? string.Empty);
                _logger.LogInformation("ApplyBasketPromo: Promo applied for UserId={UserId}", userId);

                var res = req.CreateResponse(HttpStatusCode.OK);
                await res.WriteAsJsonAsync(new { message = "Promo code applied successfully." });
                return res;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ApplyBasketPromo: Failed to apply promo for UserId={UserId}", userId);

                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteAsJsonAsync(new { error = ex.Message });
                return bad;
            }
        }

        // --- helper: extract userId from JWT ---
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
            catch { return null; }
        }
    }
}
