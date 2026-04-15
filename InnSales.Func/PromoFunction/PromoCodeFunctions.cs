
// using System.Net;
// using System.Text.Json;
// using Microsoft.Azure.Functions.Worker;
// using Microsoft.Azure.Functions.Worker.Http;
// using InnSales.Services;
// using InnSales.Common.DTO;
// using Helper;

// namespace InnSales.Functions
// {
//     public class PromoCodeFunctions
//     {
//         private readonly IPromoCodeService _promoCodes;
//         private readonly IAuthHelper _auth;

//         public PromoCodeFunctions(IPromoCodeService promoCodes, IAuthHelper auth)
//         {
//             _promoCodes = promoCodes;
//             _auth = auth;
//         }

//         [Function("CreatePromoCode")]
//         public async Task<HttpResponseData> Create(
//             [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/promocodes/create")] HttpRequestData req)
//         {
//             if (!_auth.IsAuthenticated(req))
//                 return _auth.Unauthorized(req);

//             var dto = await req.ReadFromJsonAsync<PromoCodeCreateDto>();
//             if (dto is null)
//             {
//                 var bad = req.CreateResponse(HttpStatusCode.BadRequest);
//                 await bad.WriteStringAsync("Invalid request body.");
//                 return bad;
//             }

//             var result = await _promoCodes.CreatePromoCodeAsync(dto);
//             var ok = req.CreateResponse(HttpStatusCode.OK);
//             await ok.WriteAsJsonAsync(result);
//             return ok;
//         }

//         [Function("UpdatePromoCode")]
//         public async Task<HttpResponseData> Update(
//             [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "v1/promocodes/update")] HttpRequestData req)
//         {
//             if (!_auth.IsAuthenticated(req))
//                 return _auth.Unauthorized(req);

//             var dto = await req.ReadFromJsonAsync<PromoCodeUpdateDto>();
//             if (dto is null)
//             {
//                 var bad = req.CreateResponse(HttpStatusCode.BadRequest);
//                 await bad.WriteStringAsync("Invalid request body.");
//                 return bad;
//             }

//             var result = await _promoCodes.UpdatePromoCodeAsync(dto);
//             var ok = req.CreateResponse(HttpStatusCode.OK);
//             await ok.WriteAsJsonAsync(result);
//             return ok;
//         }

//         [Function("DeletePromoCode")]
//         public async Task<HttpResponseData> Delete(
//             [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "v1/promocodes/{promoCodeId:guid}")]
//             HttpRequestData req,
//             Guid promoCodeId)
//         {
//             if (!_auth.IsAuthenticated(req))
//                 return _auth.Unauthorized(req);

//             var success = await _promoCodes.DeletePromoCodeAsync(promoCodeId);
//             var res = req.CreateResponse(success ? HttpStatusCode.OK : HttpStatusCode.NotFound);
//             if (success) await res.WriteStringAsync("Promo code deleted");
//             return res;
//         }

//         [Function("GetPromoCodeById")]
//         public async Task<HttpResponseData> GetById(
//             [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/promocodes/{promoCodeId:guid}")]
//             HttpRequestData req,
//             Guid promoCodeId)
//         {
//             if (!_auth.IsAuthenticated(req))
//                 return _auth.Unauthorized(req);

//             var result = await _promoCodes.GetPromoCodeByIdAsync(promoCodeId);
//             if (result is null)
//                 return req.CreateResponse(HttpStatusCode.NotFound);

//             var ok = req.CreateResponse(HttpStatusCode.OK);
//             await ok.WriteAsJsonAsync(result);
//             return ok;
//         }

//         [Function("GetPromoCodesByPromotion")]
//         public async Task<HttpResponseData> GetByPromotion(
//             [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/promocodes/promotion/{promotionId:guid}")]
//             HttpRequestData req,
//             Guid promotionId)
//         {
//             if (!_auth.IsAuthenticated(req))
//                 return _auth.Unauthorized(req);

//             var result = await _promoCodes.GetPromoCodesByPromotionAsync(promotionId);
//             var ok = req.CreateResponse(HttpStatusCode.OK);
//             await ok.WriteAsJsonAsync(result);
//             return ok;
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
using Helper;

namespace InnSales.Functions
{
    public class PromoCodeFunctions
    {
        private readonly IPromoCodeService _promoCodes;
        private readonly IAuthHelper _auth;
        private readonly ILogger<PromoCodeFunctions> _logger;

        public PromoCodeFunctions(
            IPromoCodeService promoCodes,
            IAuthHelper auth,
            ILogger<PromoCodeFunctions> logger)
        {
            _promoCodes = promoCodes;
            _auth = auth;
            _logger = logger;
        }

        [Function("CreatePromoCode")]
        public async Task<HttpResponseData> Create(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/promocodes/create")]
            HttpRequestData req)
        {
            _logger.LogInformation("CreatePromoCode triggered");

            if (!_auth.IsAuthenticated(req))
            {
                _logger.LogWarning("Unauthorized attempt to create promo code");
                return _auth.Unauthorized(req);
            }

            var dto = await req.ReadFromJsonAsync<PromoCodeCreateDto>();
            if (dto is null)
            {
                _logger.LogWarning("CreatePromoCode failed: invalid request body");
                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteStringAsync("Invalid request body.");
                return bad;
            }

            var result = await _promoCodes.CreatePromoCodeAsync(dto);
            _logger.LogInformation("Promo code created successfully");

            var ok = req.CreateResponse(HttpStatusCode.OK);
            await ok.WriteAsJsonAsync(result);
            return ok;
        }

        [Function("UpdatePromoCode")]
        public async Task<HttpResponseData> Update(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "v1/promocodes/update")]
            HttpRequestData req)
        {
            _logger.LogInformation("UpdatePromoCode triggered");

            if (!_auth.IsAuthenticated(req))
            {
                _logger.LogWarning("Unauthorized attempt to update promo code");
                return _auth.Unauthorized(req);
            }

            var dto = await req.ReadFromJsonAsync<PromoCodeUpdateDto>();
            if (dto is null)
            {
                _logger.LogWarning("UpdatePromoCode failed: invalid request body");
                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteStringAsync("Invalid request body.");
                return bad;
            }

            var result = await _promoCodes.UpdatePromoCodeAsync(dto);
            _logger.LogInformation("Promo code updated successfully");

            var ok = req.CreateResponse(HttpStatusCode.OK);
            await ok.WriteAsJsonAsync(result);
            return ok;
        }

        [Function("DeletePromoCode")]
        public async Task<HttpResponseData> Delete(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "v1/promocodes/{promoCodeId:guid}")]
            HttpRequestData req,
            Guid promoCodeId)
        {
            _logger.LogInformation("DeletePromoCode triggered for PromoCodeId {PromoCodeId}", promoCodeId);

            if (!_auth.IsAuthenticated(req))
            {
                _logger.LogWarning("Unauthorized attempt to delete promo code {PromoCodeId}", promoCodeId);
                return _auth.Unauthorized(req);
            }

            var success = await _promoCodes.DeletePromoCodeAsync(promoCodeId);

            if (success)
            {
                _logger.LogInformation("Promo code {PromoCodeId} deleted successfully", promoCodeId);
            }
            else
            {
                _logger.LogWarning("Promo code {PromoCodeId} not found for deletion", promoCodeId);
            }

            var res = req.CreateResponse(success ? HttpStatusCode.OK : HttpStatusCode.NotFound);
            if (success)
                await res.WriteStringAsync("Promo code deleted");

            return res;
        }

        [Function("GetPromoCodeById")]
        public async Task<HttpResponseData> GetById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/promocodes/{promoCodeId:guid}")]
            HttpRequestData req,
            Guid promoCodeId)
        {
            _logger.LogInformation("GetPromoCodeById triggered for PromoCodeId {PromoCodeId}", promoCodeId);

            if (!_auth.IsAuthenticated(req))
            {
                _logger.LogWarning("Unauthorized attempt to fetch promo code {PromoCodeId}", promoCodeId);
                return _auth.Unauthorized(req);
            }

            var result = await _promoCodes.GetPromoCodeByIdAsync(promoCodeId);
            if (result is null)
            {
                _logger.LogWarning("Promo code {PromoCodeId} not found", promoCodeId);
                return req.CreateResponse(HttpStatusCode.NotFound);
            }

            _logger.LogInformation("Promo code {PromoCodeId} retrieved successfully", promoCodeId);

            var ok = req.CreateResponse(HttpStatusCode.OK);
            await ok.WriteAsJsonAsync(result);
            return ok;
        }

        [Function("GetPromoCodesByPromotion")]
        public async Task<HttpResponseData> GetByPromotion(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/promocodes/promotion/{promotionId:guid}")]
            HttpRequestData req,
            Guid promotionId)
        {
            _logger.LogInformation(
                "GetPromoCodesByPromotion triggered for PromotionId {PromotionId}",
                promotionId);

            if (!_auth.IsAuthenticated(req))
            {
                _logger.LogWarning(
                    "Unauthorized attempt to fetch promo codes for PromotionId {PromotionId}",
                    promotionId);
                return _auth.Unauthorized(req);
            }

            var result = await _promoCodes.GetPromoCodesByPromotionAsync(promotionId);
            _logger.LogInformation(
                "Promo codes retrieved for PromotionId {PromotionId}",
                promotionId);

            var ok = req.CreateResponse(HttpStatusCode.OK);
            await ok.WriteAsJsonAsync(result);
            return ok;
        }
    }
}
