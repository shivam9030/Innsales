
// using System;
// using System.Linq;
// using System.Net;
// using System.Threading.Tasks;
// using System.Collections.Generic;
// using Microsoft.Azure.Functions.Worker;
// using Microsoft.Azure.Functions.Worker.Http;
// using Microsoft.Extensions.Logging;
// using Microsoft.Extensions.Configuration;
// using InnSales.Services;
// using InnSales.Common.DTO;
// using InnSales.Domain.Entities;
// using Helper;

// namespace InnSales.Functions
// {
//     public class ProductFunctions
//     {
//         private readonly ILogger<ProductFunctions> _logger;
//         private readonly IConfiguration _config;
//         private readonly IAuthHelper _auth;
//         private readonly IProductService _products; 
//         public ProductFunctions(
//             IProductService products,             
//             ILogger<ProductFunctions> logger,
//             IConfiguration config,
//             IAuthHelper auth)
//         {
//             _products = products;                  
//             _logger = logger;
//             _config = config;
//             _auth = auth;
//         }

//         [Function("GetAllProducts")]
//         public async Task<HttpResponseData> GetAll(
//             [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/products")] HttpRequestData req)
//         {
//             if (!_auth.IsAuthenticated(req)) return _auth.Unauthorized(req);

//             var products = await _products.GetAllAsync(); 
//             var res = req.CreateResponse(HttpStatusCode.OK);
//             await res.WriteAsJsonAsync(products);
//             return res;
//         }

//         [Function("GetProductsByCategory")]
//         public async Task<HttpResponseData> GetByCategory(
//             [HttpTrigger(AuthorizationLevel.Function, "get", Route = "v1/products/category/{categoryId:guid}")] HttpRequestData req,
//             Guid categoryId)
//         {
//             if (!_auth.IsAuthenticated(req)) return _auth.Unauthorized(req);

//             var products = await _products.GetByCategoryAsync(categoryId); 
//             var res = req.CreateResponse(HttpStatusCode.OK);
//             await res.WriteAsJsonAsync(products);
//             return res;
//         }

//         [Function("GetProductById")]
//         public async Task<HttpResponseData> GetById(
//             [HttpTrigger(AuthorizationLevel.Function, "get", Route = "v1/products/{id:guid}")] HttpRequestData req,
//             Guid id)
//         {
//             if (!_auth.IsAuthenticated(req)) return _auth.Unauthorized(req);

//             var product = await _products.GetByIdAsync(id); 
//             if (product == null) return req.CreateResponse(HttpStatusCode.NotFound);

//             var res = req.CreateResponse(HttpStatusCode.OK);
//             await res.WriteAsJsonAsync(product);
//             return res;
//         }

//         [Function("CreateProduct")]
//         public async Task<HttpResponseData> Create(
//             [HttpTrigger(AuthorizationLevel.Function, "post", Route = "v1/products")] HttpRequestData req)
//         {
//             if (!_auth.IsAuthenticated(req)) return _auth.Unauthorized(req);
//             if (!_auth.HasRole(req, "Employee")) return _auth.Forbidden(req);

//             var dto = await req.ReadFromJsonAsync<ProductDto>() ?? new ProductDto();

//             var product = new Product
//             {
//                 Id = Guid.NewGuid(),
//                 Name = dto.Name,
//                 Description = dto.Description,
//                 ImageUrl = dto.ImageUrl,
//                 Price = dto.Price,
//                 StockQuantity = dto.StockQuantity,
//                 CategoryId = dto.CategoryId,
//                 IsPromoProduct = dto.IsPromoProduct
//             };

//             var created = await _products.CreateAsync(product); 

//             var res = req.CreateResponse(HttpStatusCode.Created);
//             await res.WriteAsJsonAsync(MapToDto(created));
//             return res;
//         }

//         [Function("UpdateProduct")]
//         public async Task<HttpResponseData> Update(
//             [HttpTrigger(AuthorizationLevel.Function, "put", Route = "v1/products/{id:guid}")] HttpRequestData req,
//             Guid id)
//         {
//             if (!_auth.IsAuthenticated(req)) return _auth.Unauthorized(req);
//             if (!_auth.HasRole(req, "Employee")) return _auth.Forbidden(req);

//             var existing = await _products.GetByIdAsync(id); 
//             if (existing == null) return req.CreateResponse(HttpStatusCode.NotFound);

//             var dto = await req.ReadFromJsonAsync<ProductDto>() ?? new ProductDto();

//             existing.Name = dto.Name;
//             existing.Description = dto.Description;
//             existing.ImageUrl = dto.ImageUrl;
//             existing.Price = dto.Price;
//             existing.StockQuantity = dto.StockQuantity;
//             existing.CategoryId = dto.CategoryId;

//             var updated = await _products.UpdateAsync(id, new Product
//             {
//                 Id = id,
//                 Name = existing.Name,
//                 Description = existing.Description,
//                 ImageUrl = existing.ImageUrl,
//                 Price = existing.Price,
//                 StockQuantity = existing.StockQuantity,
//                 CategoryId = existing.CategoryId,
//                 IsPromoProduct = existing.IsPromoProduct
//             }); 

//             var res = req.CreateResponse(HttpStatusCode.OK);
//             await res.WriteAsJsonAsync(MapToDto(updated!));
//             return res;
//         }

//         [Function("DeleteProduct")]
//         public async Task<HttpResponseData> Delete(
//             [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "v1/products/{id:guid}")] HttpRequestData req,
//             Guid id)
//         {
//             if (!_auth.IsAuthenticated(req)) return _auth.Unauthorized(req);
//             if (!_auth.HasRole(req, "Employee")) return _auth.Forbidden(req);

//             var deleted = await _products.DeleteAsync(id); 
//             return deleted
//                 ? req.CreateResponse(HttpStatusCode.NoContent)
//                 : req.CreateResponse(HttpStatusCode.NotFound);
//         }

//         private static ProductDto MapToDto(Product product) => new()
//         {
//             Id = product.Id,
//             Name = product.Name,
//             Description = product.Description,
//             ImageUrl = product.IsPromoProduct ? null : product.ImageUrl,
//             Price = product.Price,
//             StockQuantity = product.IsPromoProduct ? null : product.StockQuantity,
//             CategoryId = product.CategoryId,
//             IsPromoProduct = product.IsPromoProduct
//         };
//     }
// }

using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using InnSales.Services;
using InnSales.Common.DTO;
using InnSales.Domain.Entities;
using Helper;

namespace InnSales.Functions
{
    public class ProductFunctions
    {
        private readonly ILogger<ProductFunctions> _logger;
        private readonly IConfiguration _config;
        private readonly IAuthHelper _auth;
        private readonly IProductService _products; 

        public ProductFunctions(
            IProductService products,             
            ILogger<ProductFunctions> logger,
            IConfiguration config,
            IAuthHelper auth)
        {
            _products = products;                  
            _logger = logger;
            _config = config;
            _auth = auth;
        }

        [Function("GetAllProducts")]
        public async Task<HttpResponseData> GetAll(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/products")] HttpRequestData req)
        {
            _logger.LogInformation("GetAllProducts triggered");

            if (!_auth.IsAuthenticated(req))
            {
                _logger.LogWarning("Unauthorized access to GetAllProducts");
                return _auth.Unauthorized(req);
            }

            var products = await _products.GetAllAsync(); 
            _logger.LogInformation("Fetched {Count} products", products?.Count() ?? 0);

            var res = req.CreateResponse(HttpStatusCode.OK);
            await res.WriteAsJsonAsync(products);
            _logger.LogDebug("GetAllProducts response written");
            return res;
        }

        [Function("GetProductsByCategory")]
        public async Task<HttpResponseData> GetByCategory(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "v1/products/category/{categoryId:guid}")] HttpRequestData req,
            Guid categoryId)
        {
            _logger.LogInformation("GetProductsByCategory triggered for CategoryId={CategoryId}", categoryId);

            if (!_auth.IsAuthenticated(req))
            {
                _logger.LogWarning("Unauthorized access to GetProductsByCategory for CategoryId={CategoryId}", categoryId);
                return _auth.Unauthorized(req);
            }

            var products = await _products.GetByCategoryAsync(categoryId); 
            _logger.LogInformation("Fetched {Count} products for CategoryId={CategoryId}", products?.Count() ?? 0, categoryId);

            var res = req.CreateResponse(HttpStatusCode.OK);
            await res.WriteAsJsonAsync(products);
            _logger.LogDebug("GetProductsByCategory response written for CategoryId={CategoryId}", categoryId);
            return res;
        }

        [Function("GetProductById")]
        public async Task<HttpResponseData> GetById(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "v1/products/{id:guid}")] HttpRequestData req,
            Guid id)
        {
            _logger.LogInformation("GetProductById triggered for Id={ProductId}", id);

            if (!_auth.IsAuthenticated(req))
            {
                _logger.LogWarning("Unauthorized access to GetProductById for Id={ProductId}", id);
                return _auth.Unauthorized(req);
            }

            var product = await _products.GetByIdAsync(id); 
            if (product == null)
            {
                _logger.LogInformation("Product Id={ProductId} not found. Returning 404.", id);
                return req.CreateResponse(HttpStatusCode.NotFound);
            }

            var res = req.CreateResponse(HttpStatusCode.OK);
            await res.WriteAsJsonAsync(product);
            _logger.LogDebug("GetProductById response written for Id={ProductId}", id);
            return res;
        }

        [Function("CreateProduct")]
        public async Task<HttpResponseData> Create(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "v1/products")] HttpRequestData req)
        {
            _logger.LogInformation("CreateProduct triggered");

            if (!_auth.IsAuthenticated(req))
            {
                _logger.LogWarning("Unauthorized access to CreateProduct");
                return _auth.Unauthorized(req);
            }

            if (!_auth.HasRole(req, "Employee"))
            {
                _logger.LogWarning("Forbidden: user lacks 'Employee' role for CreateProduct");
                return _auth.Forbidden(req);
            }

            var dto = await req.ReadFromJsonAsync<ProductDto>() ?? new ProductDto();
            _logger.LogDebug("CreateProduct payload read: Name={Name}, CategoryId={CategoryId}", dto?.Name, dto?.CategoryId);

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,
                Price = dto.Price,
                StockQuantity = dto.StockQuantity,
                CategoryId = dto.CategoryId,
                IsPromoProduct = dto.IsPromoProduct
            };

            var created = await _products.CreateAsync(product); 
            _logger.LogInformation("Product created with Id={ProductId}", created?.Id);

            var res = req.CreateResponse(HttpStatusCode.Created);
            await res.WriteAsJsonAsync(MapToDto(created));
            _logger.LogDebug("CreateProduct response written with Id={ProductId}", created?.Id);
            return res;
        }

        [Function("UpdateProduct")]
        public async Task<HttpResponseData> Update(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "v1/products/{id:guid}")] HttpRequestData req,
            Guid id)
        {
            _logger.LogInformation("UpdateProduct triggered for Id={ProductId}", id);

            if (!_auth.IsAuthenticated(req))
            {
                _logger.LogWarning("Unauthorized access to UpdateProduct for Id={ProductId}", id);
                return _auth.Unauthorized(req);
            }

            if (!_auth.HasRole(req, "Employee"))
            {
                _logger.LogWarning("Forbidden: user lacks 'Employee' role for UpdateProduct Id={ProductId}", id);
                return _auth.Forbidden(req);
            }

            var existing = await _products.GetByIdAsync(id); 
            if (existing == null)
            {
                _logger.LogInformation("Product Id={ProductId} not found for update. Returning 404.", id);
                return req.CreateResponse(HttpStatusCode.NotFound);
            }

            var dto = await req.ReadFromJsonAsync<ProductDto>() ?? new ProductDto();
            _logger.LogDebug("UpdateProduct payload read for Id={ProductId}: Name={Name}, CategoryId={CategoryId}", id, dto?.Name, dto?.CategoryId);

            existing.Name = dto.Name;
            existing.Description = dto.Description;
            existing.ImageUrl = dto.ImageUrl;
            existing.Price = dto.Price;
            existing.StockQuantity = dto.StockQuantity;
            existing.CategoryId = dto.CategoryId;

            var updated = await _products.UpdateAsync(id, new Product
            {
                Id = id,
                Name = existing.Name,
                Description = existing.Description,
                ImageUrl = existing.ImageUrl,
                Price = existing.Price,
                StockQuantity = existing.StockQuantity,
                CategoryId = existing.CategoryId,
                IsPromoProduct = existing.IsPromoProduct
            }); 

            _logger.LogInformation("Product updated successfully for Id={ProductId}", id);

            var res = req.CreateResponse(HttpStatusCode.OK);
            await res.WriteAsJsonAsync(MapToDto(updated!));
            _logger.LogDebug("UpdateProduct response written for Id={ProductId}", id);
            return res;
        }

        [Function("DeleteProduct")]
        public async Task<HttpResponseData> Delete(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "v1/products/{id:guid}")] HttpRequestData req,
            Guid id)
        {
            _logger.LogInformation("DeleteProduct triggered for Id={ProductId}", id);

            if (!_auth.IsAuthenticated(req))
            {
                _logger.LogWarning("Unauthorized access to DeleteProduct for Id={ProductId}", id);
                return _auth.Unauthorized(req);
            }

            if (!_auth.HasRole(req, "Employee"))
            {
                _logger.LogWarning("Forbidden: user lacks 'Employee' role for DeleteProduct Id={ProductId}", id);
                return _auth.Forbidden(req);
            }

            var deleted = await _products.DeleteAsync(id); 
            _logger.LogInformation("DeleteProduct result for Id={ProductId}: {Deleted}", id, deleted);

            return deleted
                ? req.CreateResponse(HttpStatusCode.NoContent)
                : req.CreateResponse(HttpStatusCode.NotFound);
        }

        private static ProductDto MapToDto(Product product) => new()
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            ImageUrl = product.IsPromoProduct ? null : product.ImageUrl,
            Price = product.Price,
            StockQuantity = product.IsPromoProduct ? null : product.StockQuantity,
            CategoryId = product.CategoryId,
            IsPromoProduct = product.IsPromoProduct
        };
    }
}
