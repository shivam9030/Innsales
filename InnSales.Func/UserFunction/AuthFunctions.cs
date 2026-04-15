// using Microsoft.Azure.Functions.Worker;
// using Microsoft.Azure.Functions.Worker.Http;
// using Microsoft.AspNetCore.Mvc;
// using InnSales.Services;
// using InnSales.Common.DTO.Auth;
// using System.Threading.Tasks;
// using System.Linq;
// using System.Security.Claims;
// using Microsoft.Extensions.Configuration;
// using System.Net;

// namespace InnSales.Func.UserFunction
// {
//     public class AuthFunctions
//     {
//         private readonly IAuthService _authService;
//         private readonly IConfiguration _config;

//         public AuthFunctions(IAuthService authService, IConfiguration config)
//         {
//             _authService = authService;
//             _config = config;
//         }

//         // Helper to convert IActionResult from service to HttpResponseData
//         private async Task<HttpResponseData> ToHttpResponse(HttpRequestData req, IActionResult result)
//         {
//             var response = req.CreateResponse();

//             switch (result)
//             {
//                 case ObjectResult objResult:
//                     response.StatusCode = (HttpStatusCode)(objResult.StatusCode ?? 200);
//                     if (objResult.Value != null)
//                         await response.WriteAsJsonAsync(objResult.Value);
//                     break;

//                 case OkResult:
//                     response.StatusCode = HttpStatusCode.OK;
//                     break;

//                 case UnauthorizedResult:
//                     response.StatusCode = HttpStatusCode.Unauthorized;
//                     break;

//                 default:
//                     response.StatusCode = HttpStatusCode.BadRequest;
//                     break;
//             }

//             return response;
//         }

//         [Function("RegisterUser")]
//         public async Task<HttpResponseData> Register(
//             [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v{version}/auth/register")] HttpRequestData req, int version)
//         {
//             var dto = await req.ReadFromJsonAsync<RegisterDto>();
//             var result = await _authService.RegisterAsync(dto!);
//             return await ToHttpResponse(req, result);
//         }

//         [Function("LoginUser")]
//         public async Task<HttpResponseData> Login(
//             [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v{version}/auth/login")] HttpRequestData req, int version)
//         {
//             var dto = await req.ReadFromJsonAsync<LoginDto>();
//             var result = await _authService.LoginAsync(dto!);
//             return await ToHttpResponse(req, result);
//         }

//         [Function("RefreshToken")]
//         public async Task<HttpResponseData> Refresh(
//             [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v{version}/auth/refresh")] HttpRequestData req, int version)
//         {
//             var dto = await req.ReadFromJsonAsync<RefreshTokenDto>();
//             var result = await _authService.RefreshTokenAsync(dto!);
//             return await ToHttpResponse(req, result);
//         }

//         [Function("GetCurrentUser")]
//         public async Task<HttpResponseData> Me(
//             [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v{version}/auth/me")] HttpRequestData req, int version)
//         {
//             var authHeader = req.Headers.GetValues("Authorization")?.FirstOrDefault();
//             if (authHeader == null || !authHeader.StartsWith("Bearer "))
//             {
//                 var unauthorized = req.CreateResponse(HttpStatusCode.Unauthorized);
//                 return unauthorized;
//             }

//             var token = authHeader.Replace("Bearer ", "");
//             var principal = JwtHelper.Validate(token, _config);
//             if (principal == null)                  
//             {
//                 var unauthorized = req.CreateResponse(HttpStatusCode.Unauthorized);
//                 return unauthorized;
//             }

//             var result = await _authService.GetCurrentUserAsync(principal);
//             return await ToHttpResponse(req, result);
//         }
//     }
// }

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.AspNetCore.Mvc;
using InnSales.Services;
using InnSales.Common.DTO.Auth;
using System.Threading.Tasks;
using System.Linq;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging; // ✅ added
using System.Net;

namespace InnSales.Func.UserFunction
{
    public class AuthFunctions
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthFunctions> _logger; // ✅ added

        public AuthFunctions(IAuthService authService, IConfiguration config, ILogger<AuthFunctions> logger) // ✅ logger injected
        {
            _authService = authService;
            _config = config;
            _logger = logger;
        }

        // Helper to convert IActionResult from service to HttpResponseData
        private async Task<HttpResponseData> ToHttpResponse(HttpRequestData req, IActionResult result)
        {
            _logger.LogDebug("ToHttpResponse: Converting {ResultType}", result?.GetType().Name ?? "null");

            var response = req.CreateResponse();

            switch (result)
            {
                case ObjectResult objResult:
                    response.StatusCode = (HttpStatusCode)(objResult.StatusCode ?? 200);
                    _logger.LogDebug("ToHttpResponse: ObjectResult -> Status={Status}", response.StatusCode);
                    if (objResult.Value != null)
                    {
                        await response.WriteAsJsonAsync(objResult.Value);
                        _logger.LogTrace("ToHttpResponse: ObjectResult payload written");
                    }
                    break;

                case OkResult:
                    response.StatusCode = HttpStatusCode.OK;
                    _logger.LogDebug("ToHttpResponse: OkResult -> Status=200");
                    break;

                case UnauthorizedResult:
                    response.StatusCode = HttpStatusCode.Unauthorized;
                    _logger.LogDebug("ToHttpResponse: UnauthorizedResult -> Status=401");
                    break;

                default:
                    response.StatusCode = HttpStatusCode.BadRequest;
                    _logger.LogDebug("ToHttpResponse: Default -> Status=400");
                    break;
            }

            return response;
        }

        [Function("RegisterUser")]
        public async Task<HttpResponseData> Register(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v{version}/auth/register")] HttpRequestData req, int version)
        {
            _logger.LogInformation("RegisterUser triggered (v{Version})", version);

            var dto = await req.ReadFromJsonAsync<RegisterDto>();
            _logger.LogDebug("RegisterUser: Payload read - Email={Email}", dto?.Email);

            var result = await _authService.RegisterAsync(dto!);
            _logger.LogInformation("RegisterUser: Service completed");

            var res = await ToHttpResponse(req, result);
            _logger.LogDebug("RegisterUser: Response written with HTTP {StatusCode}", res.StatusCode);
            return res;
        }

        [Function("LoginUser")]
        public async Task<HttpResponseData> Login(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v{version}/auth/login")] HttpRequestData req, int version)
        {
            _logger.LogInformation("LoginUser triggered (v{Version})", version);

            var dto = await req.ReadFromJsonAsync<LoginDto>();
            _logger.LogDebug("LoginUser: Payload read - Email={Email}", dto?.Email);

            var result = await _authService.LoginAsync(dto!);
            _logger.LogInformation("LoginUser: Service completed");

            var res = await ToHttpResponse(req, result);
            _logger.LogDebug("LoginUser: Response written with HTTP {StatusCode}", res.StatusCode);
            return res;
        }

        [Function("RefreshToken")]
        public async Task<HttpResponseData> Refresh(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v{version}/auth/refresh")] HttpRequestData req, int version)
        {
            _logger.LogInformation("RefreshToken triggered (v{Version})", version);

            var dto = await req.ReadFromJsonAsync<RefreshTokenDto>();
            _logger.LogDebug("RefreshToken: Payload read - Token begins with={Prefix}",
                dto?.RefreshToken is string t && t.Length >= 8 ? t[..8] : "null");

            var result = await _authService.RefreshTokenAsync(dto!);
            _logger.LogInformation("RefreshToken: Service completed");

            var res = await ToHttpResponse(req, result);
            _logger.LogDebug("RefreshToken: Response written with HTTP {StatusCode}", res.StatusCode);
            return res;
        }

        [Function("GetCurrentUser")]
        public async Task<HttpResponseData> Me(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v{version}/auth/me")] HttpRequestData req, int version)
        {
            _logger.LogInformation("GetCurrentUser triggered (v{Version})", version);

            var authHeader = req.Headers.GetValues("Authorization")?.FirstOrDefault();
            if (authHeader == null || !authHeader.StartsWith("Bearer "))
            {
                _logger.LogWarning("GetCurrentUser: Missing or invalid Authorization header");
                var unauthorized = req.CreateResponse(HttpStatusCode.Unauthorized);
                return unauthorized;
            }

            var token = authHeader.Replace("Bearer ", "");
            _logger.LogDebug("GetCurrentUser: Bearer token length={Length}", token?.Length ?? 0);

            var principal = JwtHelper.Validate(token, _config);
            if (principal == null)
            {
                _logger.LogWarning("GetCurrentUser: JWT validation failed");
                var unauthorized = req.CreateResponse(HttpStatusCode.Unauthorized);
                return unauthorized;
            }

            _logger.LogInformation("GetCurrentUser: Principal validated. Claims={Count}", principal.Claims?.Count() ?? 0);

            var result = await _authService.GetCurrentUserAsync(principal);
            _logger.LogInformation("GetCurrentUser: Service completed");

            var res = await ToHttpResponse(req, result);
            _logger.LogDebug("GetCurrentUser: Response written with HTTP {StatusCode}", res.StatusCode);
            return res;
        }
    }
}
