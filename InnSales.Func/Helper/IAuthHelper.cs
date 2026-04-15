
using Microsoft.Azure.Functions.Worker.Http;

namespace Helper
{
    public interface IAuthHelper
    {
        bool IsAuthenticated(HttpRequestData req);
        bool HasRole(HttpRequestData req, string role);
        HttpResponseData Unauthorized(HttpRequestData req);
        HttpResponseData Forbidden(HttpRequestData req);
    }
}
