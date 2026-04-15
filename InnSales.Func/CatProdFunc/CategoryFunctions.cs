
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
// using InnSales.Domain.Entities;
// using InnSales.Common.DTO;
// using Helper;

// namespace InnSales.Functions
// {
//     public class CategoryFunctions
//     {
//         private readonly ILogger<CategoryFunctions> _logger;
//         private readonly IConfiguration _config;
//         private readonly IAuthHelper _auth;
//         private readonly ICategoryService _categories; 
//         public CategoryFunctions(
//             ICategoryService categories,          
//             ILogger<CategoryFunctions> logger,
//             IConfiguration config,
//             IAuthHelper auth)
//         {
//             _categories = categories;           
//             _logger = logger;
//             _config = config;
//             _auth = auth;
//         }

//         [Function("GetAllCategories")]
//         public async Task<HttpResponseData> GetAll(
//             [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/categories")] HttpRequestData req)
//         {
//             if (!_auth.IsAuthenticated(req)) return _auth.Unauthorized(req);

//             var categories = await _categories.GetAllCategoriesAsync();
//             var res = req.CreateResponse(HttpStatusCode.OK);
//             await res.WriteAsJsonAsync(categories.Select(MapToDto));
//             return res;
//         }

//         [Function("GetCategoryById")]
//         public async Task<HttpResponseData> GetById(
//             [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/categories/{id:guid}")] HttpRequestData req,
//             Guid id)
//         {
//             if (!_auth.IsAuthenticated(req)) return _auth.Unauthorized(req);

//             try
//             {
//                 var category = await _categories.GetCategoryByIdAsync(id); 
//                 if (category.IsDeleted) return req.CreateResponse(HttpStatusCode.NotFound);

//                 var res = req.CreateResponse(HttpStatusCode.OK);
//                 await res.WriteAsJsonAsync(MapToDto(category));
//                 return res;
//             }
//             catch (KeyNotFoundException)
//             {
//                 return req.CreateResponse(HttpStatusCode.NotFound);
//             }
//         }

//         [Function("CreateCategory")]
//         public async Task<HttpResponseData> Create(
//             [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/categories")] HttpRequestData req)
//         {
//             if (!_auth.IsAuthenticated(req)) return _auth.Unauthorized(req);
//             if (!_auth.HasRole(req, "Employee")) return _auth.Forbidden(req);

//             var dto = await req.ReadFromJsonAsync<CategoryDto>() ?? new CategoryDto();

//             var category = new Category
//             {
//                 Id = Guid.NewGuid(),
//                 Name = dto.Name,
//                 Description = dto.Description,
//                 ImageUrl = dto.ImageUrl,
//                 IsDeleted = false
//             };

//             var id = await _categories.CreateCategoryAsync(category);

//             var res = req.CreateResponse(HttpStatusCode.Created);
//             await res.WriteAsJsonAsync(new { id });
//             return res;
//         }

//         [Function("UpdateCategory")]
//         public async Task<HttpResponseData> Update(
//             [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "v1/categories/{id:guid}")] HttpRequestData req,
//             Guid id)
//         {
//             if (!_auth.IsAuthenticated(req)) return _auth.Unauthorized(req);
//             if (!_auth.HasRole(req, "Employee")) return _auth.Forbidden(req);

//             var dto = await req.ReadFromJsonAsync<CategoryDto>() ?? new CategoryDto();

//             try
//             {
//                 var updatedCategory = new Category
//                 {
//                     Id = id,
//                     Name = dto.Name,
//                     Description = dto.Description,
//                     ImageUrl = dto.ImageUrl
//                 };

//                 await _categories.UpdateCategoryAsync(id, updatedCategory); 
//                 return req.CreateResponse(HttpStatusCode.NoContent);
//             }
//             catch (KeyNotFoundException)
//             {
//                 return req.CreateResponse(HttpStatusCode.NotFound);
//             }
//         }

//         [Function("DeleteCategory")]
//         public async Task<HttpResponseData> Delete(
//             [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "v1/categories/{id:guid}")] HttpRequestData req,
//             Guid id)
//         {
//             if (!_auth.IsAuthenticated(req)) return _auth.Unauthorized(req);
//             if (!_auth.HasRole(req, "Employee")) return _auth.Forbidden(req);

//             try
//             {
//                 await _categories.DeleteCategoryAsync(id);  
//                 return req.CreateResponse(HttpStatusCode.NoContent);
//             }
//             catch (KeyNotFoundException)
//             {
//                 return req.CreateResponse(HttpStatusCode.NotFound);
//             }
//         }

//         private static CategoryDto MapToDto(Category category) => new()
//         {
//             Id = category.Id,
//             Name = category.Name,
//             Description = category.Description,
//             ImageUrl = category.ImageUrl
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
using InnSales.Domain.Entities;
using InnSales.Common.DTO;
using Helper;

namespace InnSales.Functions
{
    public class CategoryFunctions
    {
        private readonly ILogger<CategoryFunctions> _logger;
        private readonly IConfiguration _config;
        private readonly IAuthHelper _auth;
        private readonly ICategoryService _categories; 

        public CategoryFunctions(
            ICategoryService categories,          
            ILogger<CategoryFunctions> logger,
            IConfiguration config,
            IAuthHelper auth)
        {
            _categories = categories;           
            _logger = logger;
            _config = config;
            _auth = auth;
        }

        [Function("GetAllCategories")]
        public async Task<HttpResponseData> GetAll(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/categories")] HttpRequestData req)
        {
            _logger.LogInformation("GetAllCategories triggered");

            if (!_auth.IsAuthenticated(req))
            {
                _logger.LogWarning("Unauthorized access to GetAllCategories");
                return _auth.Unauthorized(req);
            }

            var categories = await _categories.GetAllCategoriesAsync();
            _logger.LogInformation("Fetched {Count} categories", categories?.Count() ?? 0);

            var res = req.CreateResponse(HttpStatusCode.OK);
            await res.WriteAsJsonAsync(categories.Select(MapToDto));
            _logger.LogDebug("GetAllCategories response written");
            return res;
        }

        [Function("GetCategoryById")]
        public async Task<HttpResponseData> GetById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/categories/{id:guid}")] HttpRequestData req,
            Guid id)
        {
            _logger.LogInformation("GetCategoryById triggered for Id={CategoryId}", id);

            if (!_auth.IsAuthenticated(req))
            {
                _logger.LogWarning("Unauthorized access to GetCategoryById for Id={CategoryId}", id);
                return _auth.Unauthorized(req);
            }

            try
            {
                var category = await _categories.GetCategoryByIdAsync(id); 
                if (category.IsDeleted)
                {
                    _logger.LogInformation("Category Id={CategoryId} is marked deleted. Returning 404.", id);
                    return req.CreateResponse(HttpStatusCode.NotFound);
                }

                var res = req.CreateResponse(HttpStatusCode.OK);
                await res.WriteAsJsonAsync(MapToDto(category));
                _logger.LogDebug("GetCategoryById response written for Id={CategoryId}", id);
                return res;
            }
            catch (KeyNotFoundException)
            {
                _logger.LogInformation("Category Id={CategoryId} not found. Returning 404.", id);
                return req.CreateResponse(HttpStatusCode.NotFound);
            }
        }

        [Function("CreateCategory")]
        public async Task<HttpResponseData> Create(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/categories")] HttpRequestData req)
        {
            _logger.LogInformation("CreateCategory triggered");

            if (!_auth.IsAuthenticated(req))
            {
                _logger.LogWarning("Unauthorized access to CreateCategory");
                return _auth.Unauthorized(req);
            }

            if (!_auth.HasRole(req, "Employee"))
            {
                _logger.LogWarning("Forbidden: user lacks 'Employee' role for CreateCategory");
                return _auth.Forbidden(req);
            }

            var dto = await req.ReadFromJsonAsync<CategoryDto>() ?? new CategoryDto();
            _logger.LogDebug("CreateCategory payload read: Name={Name}", dto?.Name);

            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,
                IsDeleted = false
            };

            var id = await _categories.CreateCategoryAsync(category);
            _logger.LogInformation("Category created with Id={CategoryId}", id);

            var res = req.CreateResponse(HttpStatusCode.Created);
            await res.WriteAsJsonAsync(new { id });
            _logger.LogDebug("CreateCategory response written with Id={CategoryId}", id);
            return res;
        }

        [Function("UpdateCategory")]
        public async Task<HttpResponseData> Update(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "v1/categories/{id:guid}")] HttpRequestData req,
            Guid id)
        {
            _logger.LogInformation("UpdateCategory triggered for Id={CategoryId}", id);

            if (!_auth.IsAuthenticated(req))
            {
                _logger.LogWarning("Unauthorized access to UpdateCategory for Id={CategoryId}", id);
                return _auth.Unauthorized(req);
            }

            if (!_auth.HasRole(req, "Employee"))
            {
                _logger.LogWarning("Forbidden: user lacks 'Employee' role for UpdateCategory Id={CategoryId}", id);
                return _auth.Forbidden(req);
            }

            var dto = await req.ReadFromJsonAsync<CategoryDto>() ?? new CategoryDto();
            _logger.LogDebug("UpdateCategory payload read for Id={CategoryId}: Name={Name}", id, dto?.Name);

            try
            {
                var updatedCategory = new Category
                {
                    Id = id,
                    Name = dto.Name,
                    Description = dto.Description,
                    ImageUrl = dto.ImageUrl
                };

                await _categories.UpdateCategoryAsync(id, updatedCategory); 
                _logger.LogInformation("Category updated successfully for Id={CategoryId}", id);
                return req.CreateResponse(HttpStatusCode.NoContent);
            }
            catch (KeyNotFoundException)
            {
                _logger.LogInformation("Category Id={CategoryId} not found for update. Returning 404.", id);
                return req.CreateResponse(HttpStatusCode.NotFound);
            }
        }

        [Function("DeleteCategory")]
        public async Task<HttpResponseData> Delete(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "v1/categories/{id:guid}")] HttpRequestData req,
            Guid id)
        {
            _logger.LogInformation("DeleteCategory triggered for Id={CategoryId}", id);

            if (!_auth.IsAuthenticated(req))
            {
                _logger.LogWarning("Unauthorized access to DeleteCategory for Id={CategoryId}", id);
                return _auth.Unauthorized(req);
            }

            if (!_auth.HasRole(req, "Employee"))
            {
                _logger.LogWarning("Forbidden: user lacks 'Employee' role for DeleteCategory Id={CategoryId}", id);
                return _auth.Forbidden(req);
            }

            try
            {
                await _categories.DeleteCategoryAsync(id);  
                _logger.LogInformation("Category deleted (soft/hard per service) for Id={CategoryId}", id);
                return req.CreateResponse(HttpStatusCode.NoContent);
            }
            catch (KeyNotFoundException)
            {
                _logger.LogInformation("Category Id={CategoryId} not found for delete. Returning 404.", id);
                return req.CreateResponse(HttpStatusCode.NotFound);
            }
        }

        private static CategoryDto MapToDto(Category category) => new()
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            ImageUrl = category.ImageUrl
        };
    }
}
