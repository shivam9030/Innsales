
// using System;
// using System.Linq;
// using System.Net;
// using System.Threading.Tasks;
// using System.Collections.Generic;
// using Microsoft.Azure.Functions.Worker;
// using Microsoft.Azure.Functions.Worker.Http;
// using Microsoft.Extensions.Logging;
// using Microsoft.EntityFrameworkCore;
// using InnSales.DataBase;
// using InnSales.Domain.Entities;
// using Helper; // IAuthHelper

// namespace InnSales.Functions
// {
//     public class FailedTransactionFunction
//     {
//         private readonly ILogger<FailedTransactionFunction> _logger;
//         private readonly InnSalesDbContext _db;
//         private readonly IAuthHelper _auth;

//         public FailedTransactionFunction(
//             ILogger<FailedTransactionFunction> logger,
//             InnSalesDbContext db,
//             IAuthHelper auth)
//         {
//             _logger = logger;
//             _db = db;
//             _auth = auth;
//         }

//         // GET /api/v1/failed-transactions
//         [Function("GetAllFailedTransactions")]
//         public async Task<HttpResponseData> GetAllFailedTransactions(
//             [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/failed-transactions")] HttpRequestData req)
//         {
//             // Auth: must be authenticated and have role Employee
//             if (!_auth.IsAuthenticated(req))
//                 return _auth.Unauthorized(req);
//             if (!_auth.HasRole(req, "Employee"))
//                 return _auth.Forbidden(req);

//             _logger.LogInformation("Listing all failed transactions");

//             var list = await _db.FailedTransactions
//                 .OrderByDescending(ft => ft.LoggedDate)
//                 .ToListAsync();

//             var res = req.CreateResponse(HttpStatusCode.OK);
//             await res.WriteAsJsonAsync(list);
//             return res;
//         }

//         // GET /api/v1/failed-transactions/{id}
//         [Function("GetFailedTransactionById")]
//         public async Task<HttpResponseData> GetFailedTransactionById(
//             [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/failed-transactions/{id:guid}")] HttpRequestData req,
//             Guid id)
//         {
//             // Auth: must be authenticated and have role Employee
//             if (!_auth.IsAuthenticated(req))
//                 return _auth.Unauthorized(req);
//             if (!_auth.HasRole(req, "Employee"))
//                 return _auth.Forbidden(req);

//             _logger.LogInformation("Fetching failed transaction {Id}", id);

//             var tx = await _db.FailedTransactions.FindAsync(id);
//             if (tx is null)
//                 return req.CreateResponse(HttpStatusCode.NotFound);

//             var res = req.CreateResponse(HttpStatusCode.OK);
//             await res.WriteAsJsonAsync(tx);
//             return res;
//         }
//     }
// }
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using InnSales.DataBase;
using InnSales.Domain.Entities;
using Helper; // IAuthHelper

namespace InnSales.Functions
{
    public class FailedTransactionFunction
    {
        private readonly ILogger<FailedTransactionFunction> _logger;
        private readonly InnSalesDbContext _db;
        private readonly IAuthHelper _auth;

        public FailedTransactionFunction(
            ILogger<FailedTransactionFunction> logger,
            InnSalesDbContext db,
            IAuthHelper auth)
        {
            _logger = logger;
            _db = db;
            _auth = auth;
        }

        // GET /api/v1/failed-transactions
        [Function("GetAllFailedTransactions")]
        public async Task<HttpResponseData> GetAllFailedTransactions(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/failed-transactions")] HttpRequestData req)
        {
            _logger.LogInformation("Received request to list all failed transactions");

            // Auth: must be authenticated and have role Employee
            if (!_auth.IsAuthenticated(req))
            {
                _logger.LogWarning("Unauthorized access attempt to list failed transactions");
                return _auth.Unauthorized(req);
            }

            if (!_auth.HasRole(req, "Employee"))
            {
                _logger.LogWarning("Forbidden access attempt: user lacks Employee role");
                return _auth.Forbidden(req);
            }

            _logger.LogInformation("Fetching failed transactions from database");

            var list = await _db.FailedTransactions
                .OrderByDescending(ft => ft.LoggedDate)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} failed transactions", list.Count);

            var res = req.CreateResponse(HttpStatusCode.OK);
            await res.WriteAsJsonAsync(list);
            return res;
        }

        // GET /api/v1/failed-transactions/{id}
        [Function("GetFailedTransactionById")]
        public async Task<HttpResponseData> GetFailedTransactionById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/failed-transactions/{id:guid}")] HttpRequestData req,
            Guid id)
        {
            _logger.LogInformation("Received request to fetch failed transaction {Id}", id);

            // Auth: must be authenticated and have role Employee
            if (!_auth.IsAuthenticated(req))
            {
                _logger.LogWarning("Unauthorized access attempt to fetch transaction {Id}", id);
                return _auth.Unauthorized(req);
            }

            if (!_auth.HasRole(req, "Employee"))
            {
                _logger.LogWarning("Forbidden access attempt to fetch transaction {Id}: user lacks Employee role", id);
                return _auth.Forbidden(req);
            }

            var tx = await _db.FailedTransactions.FindAsync(id);

            if (tx is null)
            {
                _logger.LogWarning("Failed transaction {Id} not found", id);
                return req.CreateResponse(HttpStatusCode.NotFound);
            }

            _logger.LogInformation("Successfully fetched failed transaction {Id}", id);

            var res = req.CreateResponse(HttpStatusCode.OK);
            await res.WriteAsJsonAsync(tx);
            return res;
        }
    }
}
