
// using System.Net;
// using System.Text.Json;
// using Microsoft.Azure.Functions.Worker;
// using Microsoft.Azure.Functions.Worker.Http;
// using InnSales.Services;
// using InnSales.Common.DTO;
// using InnSales.Common.Enums;
// using Helper;

// namespace InnSales.Functions
// {
//     public class PromotionFunctions
//     {
//         private readonly IPromotionService _promotions;
//         private readonly IAuthHelper _auth;

//         public PromotionFunctions(IPromotionService promotions, IAuthHelper auth)
//         {
//             _promotions = promotions;
//             _auth = auth;
//         }

//         // POST: v1/promotions/create
//         [Function("CreatePromotion")]
//         public async Task<HttpResponseData> Create(
//             [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/promotions/create")] HttpRequestData req)
//         {
//             if (!_auth.IsAuthenticated(req))
//                 return _auth.Unauthorized(req);

//             var dto = await req.ReadFromJsonAsync<PromotionCreateDto>();
//             if (dto is null)
//             {
//                 var bad = req.CreateResponse(HttpStatusCode.BadRequest);
//                 await bad.WriteStringAsync("Invalid request body.");
//                 return bad;
//             }

//             var result = await _promotions.CreatePromotionAsync(dto);
//             var ok = req.CreateResponse(HttpStatusCode.OK);
//             await ok.WriteAsJsonAsync(result);
//             return ok;
//         }

//         // PUT: v1/promotions/update
//         [Function("UpdatePromotion")]
//         public async Task<HttpResponseData> Update(
//             [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "v1/promotions/update")] HttpRequestData req)
//         {
//             if (!_auth.IsAuthenticated(req))
//                 return _auth.Unauthorized(req);

//             var dto = await req.ReadFromJsonAsync<PromotionUpdateDto>();
//             if (dto is null)
//             {
//                 var bad = req.CreateResponse(HttpStatusCode.BadRequest);
//                 await bad.WriteStringAsync("Invalid request body.");
//                 return bad;
//             }

//             var result = await _promotions.UpdatePromotionAsync(dto);
//             var ok = req.CreateResponse(HttpStatusCode.OK);
//             await ok.WriteAsJsonAsync(result);
//             return ok;
//         }

//         // DELETE: v1/promotions/{promotionId}
//         [Function("DeletePromotion")]
//         public async Task<HttpResponseData> Delete(
//             [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "v1/promotions/{promotionId:guid}")]
//             HttpRequestData req,
//             Guid promotionId)
//         {
//             if (!_auth.IsAuthenticated(req))
//                 return _auth.Unauthorized(req);

//             var success = await _promotions.DeletePromotionAsync(promotionId);
//             var res = req.CreateResponse(success ? HttpStatusCode.OK : HttpStatusCode.NotFound);
//             if (success) await res.WriteStringAsync("Promotion deleted");
//             return res;
//         }

//         // GET: v1/promotions/{promotionId}
//         [Function("GetPromotionById")]
//         public async Task<HttpResponseData> GetById(
//             [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/promotions/{promotionId:guid}")]
//             HttpRequestData req,
//             Guid promotionId)
//         {
//             if (!_auth.IsAuthenticated(req))
//                 return _auth.Unauthorized(req);

//             var result = await _promotions.GetPromotionByIdAsync(promotionId);
//             if (result is null)
//                 return req.CreateResponse(HttpStatusCode.NotFound);

//             var ok = req.CreateResponse(HttpStatusCode.OK);
//             await ok.WriteAsJsonAsync(result);
//             return ok;
//         }

//         // GET: v1/promotions
//         [Function("GetAllPromotions")]
//         public async Task<HttpResponseData> GetAll(
//             [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/promotions")] HttpRequestData req)
//         {
//             if (!_auth.IsAuthenticated(req))
//                 return _auth.Unauthorized(req);

//             var result = await _promotions.GetAllPromotionsAsync();
//             var ok = req.CreateResponse(HttpStatusCode.OK);
//             await ok.WriteAsJsonAsync(result);
//             return ok;
//         }

//         // PATCH: v1/promotions/{promotionId}/status?status=Active|Inactive|Paused|Expired
//         [Function("ChangePromotionStatus")]
//         public async Task<HttpResponseData> ChangeStatus(
//             [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "v1/promotions/{promotionId:guid}/status")] HttpRequestData req,
//             Guid promotionId)
//         {
//             if (!_auth.IsAuthenticated(req))
//                 return _auth.Unauthorized(req);

//             var statusValue = GetQueryParam(req, "status");
//             if (string.IsNullOrWhiteSpace(statusValue) ||
//                 !Enum.TryParse<PromotionStatus>(statusValue, true, out var status))
//             {
//                 var bad = req.CreateResponse(HttpStatusCode.BadRequest);
//                 await bad.WriteStringAsync("Invalid or missing 'status' query parameter.");
//                 return bad;
//             }

//             var success = await _promotions.ChangePromotionStatusAsync(promotionId, status);
//             var res = req.CreateResponse(success ? HttpStatusCode.OK : HttpStatusCode.NotFound);
//             if (success) await res.WriteStringAsync("Status updated");
//             return res;
//         }

//         private static string? GetQueryParam(HttpRequestData req, string key)
//         {
//             var q = req.Url.Query; // e.g., "?status=Active"
//             if (string.IsNullOrEmpty(q)) return null;
//             if (q.StartsWith("?")) q = q[1..];
//             foreach (var pair in q.Split('&', StringSplitOptions.RemoveEmptyEntries))
//             {
//                 var kv = pair.Split('=', 2);
//                 if (kv.Length == 2 &&
//                     string.Equals(Uri.UnescapeDataString(kv[0]), key, StringComparison.OrdinalIgnoreCase))
//                 {
//                     return Uri.UnescapeDataString(kv[1]);
//                 }
//             }
//             return null;
//         }
//     }
// }
using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using InnSales.Services;
using InnSales.Common.DTO;
using InnSales.Common.Enums;
using Helper;

namespace InnSales.Functions
{
    public class PromotionFunctions
    {
        private readonly IPromotionService _promotions;
        private readonly IAuthHelper _auth;
        private readonly ILogger<PromotionFunctions> _logger;

        public PromotionFunctions(
            IPromotionService promotions,
            IAuthHelper auth,
            ILogger<PromotionFunctions> logger)
        {
            _promotions = promotions;
            _auth = auth;
            _logger = logger;
        }

        // POST: v1/promotions/create
        [Function("CreatePromotion")]
        public async Task<HttpResponseData> Create(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/promotions/create")]
            HttpRequestData req)
        {
            _logger.LogInformation("CreatePromotion triggered");

            if (!_auth.IsAuthenticated(req))
            {
                _logger.LogWarning("Unauthorized attempt to create promotion");
                return _auth.Unauthorized(req);
            }

            var dto = await req.ReadFromJsonAsync<PromotionCreateDto>();
            if (dto is null)
            {
                _logger.LogWarning("CreatePromotion failed: invalid request body");
                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteStringAsync("Invalid request body.");
                return bad;
            }

            var result = await _promotions.CreatePromotionAsync(dto);
            _logger.LogInformation("Promotion created successfully");

            var ok = req.CreateResponse(HttpStatusCode.OK);
            await ok.WriteAsJsonAsync(result);
            return ok;
        }

        // PUT: v1/promotions/update
        [Function("UpdatePromotion")]
        public async Task<HttpResponseData> Update(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "v1/promotions/update")]
            HttpRequestData req)
        {
            _logger.LogInformation("UpdatePromotion triggered");

            if (!_auth.IsAuthenticated(req))
            {
                _logger.LogWarning("Unauthorized attempt to update promotion");
                return _auth.Unauthorized(req);
            }

            var dto = await req.ReadFromJsonAsync<PromotionUpdateDto>();
            if (dto is null)
            {
                _logger.LogWarning("UpdatePromotion failed: invalid request body");
                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteStringAsync("Invalid request body.");
                return bad;
            }

            var result = await _promotions.UpdatePromotionAsync(dto);
            _logger.LogInformation("Promotion updated successfully");

            var ok = req.CreateResponse(HttpStatusCode.OK);
            await ok.WriteAsJsonAsync(result);
            return ok;
        }

        // DELETE: v1/promotions/{promotionId}
        [Function("DeletePromotion")]
        public async Task<HttpResponseData> Delete(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "v1/promotions/{promotionId:guid}")]
            HttpRequestData req,
            Guid promotionId)
        {
            _logger.LogInformation(
                "DeletePromotion triggered for PromotionId {PromotionId}",
                promotionId);

            if (!_auth.IsAuthenticated(req))
            {
                _logger.LogWarning(
                    "Unauthorized attempt to delete promotion {PromotionId}",
                    promotionId);
                return _auth.Unauthorized(req);
            }

            var success = await _promotions.DeletePromotionAsync(promotionId);

            if (success)
            {
                _logger.LogInformation(
                    "Promotion {PromotionId} deleted successfully",
                    promotionId);
            }
            else
            {
                _logger.LogWarning(
                    "Promotion {PromotionId} not found for deletion",
                    promotionId);
            }

            var res = req.CreateResponse(success ? HttpStatusCode.OK : HttpStatusCode.NotFound);
            if (success)
                await res.WriteStringAsync("Promotion deleted");

            return res;
        }

        // GET: v1/promotions/{promotionId}
        [Function("GetPromotionById")]
        public async Task<HttpResponseData> GetById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/promotions/{promotionId:guid}")]
            HttpRequestData req,
            Guid promotionId)
        {
            _logger.LogInformation(
                "GetPromotionById triggered for PromotionId {PromotionId}",
                promotionId);

            if (!_auth.IsAuthenticated(req))
            {
                _logger.LogWarning(
                    "Unauthorized attempt to fetch promotion {PromotionId}",
                    promotionId);
                return _auth.Unauthorized(req);
            }

            var result = await _promotions.GetPromotionByIdAsync(promotionId);
            if (result is null)
            {
                _logger.LogWarning(
                    "Promotion {PromotionId} not found",
                    promotionId);
                return req.CreateResponse(HttpStatusCode.NotFound);
            }

            _logger.LogInformation(
                "Promotion {PromotionId} retrieved successfully",
                promotionId);

            var ok = req.CreateResponse(HttpStatusCode.OK);
            await ok.WriteAsJsonAsync(result);
            return ok;
        }

        // GET: v1/promotions
        [Function("GetAllPromotions")]
        public async Task<HttpResponseData> GetAll(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/promotions")]
            HttpRequestData req)
        {
            _logger.LogInformation("GetAllPromotions triggered");

            if (!_auth.IsAuthenticated(req))
            {
                _logger.LogWarning("Unauthorized attempt to fetch all promotions");
                return _auth.Unauthorized(req);
            }

            var result = await _promotions.GetAllPromotionsAsync();
            _logger.LogInformation("All promotions retrieved successfully");

            var ok = req.CreateResponse(HttpStatusCode.OK);
            await ok.WriteAsJsonAsync(result);
            return ok;
        }

        // PATCH: v1/promotions/{promotionId}/status
        [Function("ChangePromotionStatus")]
        public async Task<HttpResponseData> ChangeStatus(
            [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "v1/promotions/{promotionId:guid}/status")]
            HttpRequestData req,
            Guid promotionId)
        {
            _logger.LogInformation(
                "ChangePromotionStatus triggered for PromotionId {PromotionId}",
                promotionId);

            if (!_auth.IsAuthenticated(req))
            {
                _logger.LogWarning(
                    "Unauthorized attempt to change status for PromotionId {PromotionId}",
                    promotionId);
                return _auth.Unauthorized(req);
            }

            var statusValue = GetQueryParam(req, "status");
            if (string.IsNullOrWhiteSpace(statusValue) ||
                !Enum.TryParse<PromotionStatus>(statusValue, true, out var status))
            {
                _logger.LogWarning(
                    "Invalid status value '{Status}' for PromotionId {PromotionId}",
                    statusValue,
                    promotionId);

                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteStringAsync("Invalid or missing 'status' query parameter.");
                return bad;
            }

            var success = await _promotions.ChangePromotionStatusAsync(promotionId, status);

            if (success)
            {
                _logger.LogInformation(
                    "Promotion {PromotionId} status changed to {Status}",
                    promotionId,
                    status);
            }
            else
            {
                _logger.LogWarning(
                    "Promotion {PromotionId} not found while changing status",
                    promotionId);
            }

            var res = req.CreateResponse(success ? HttpStatusCode.OK : HttpStatusCode.NotFound);
            if (success)
                await res.WriteStringAsync("Status updated");

            return res;
        }

        private static string? GetQueryParam(HttpRequestData req, string key)
        {
            var q = req.Url.Query;
            if (string.IsNullOrEmpty(q)) return null;
            if (q.StartsWith("?")) q = q[1..];

            foreach (var pair in q.Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                var kv = pair.Split('=', 2);
                if (kv.Length == 2 &&
                    string.Equals(Uri.UnescapeDataString(kv[0]), key, StringComparison.OrdinalIgnoreCase))
                {
                    return Uri.UnescapeDataString(kv[1]);
                }
            }
            return null;
        }
    }
}
