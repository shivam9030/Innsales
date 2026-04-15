
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using InnSales.Services;
using InnSales.Common.DTO;
using InnSales.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InnSales.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/categories")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<CategoryDto>>> GetAllCategories()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            var categoryDtos = categories.Select(MapToDto).ToList();
            return Ok(categoryDtos);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDto>> GetCategoryById(Guid id)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                if (category.IsDeleted) return NotFound();
                return Ok(MapToDto(category));
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Category not found.");
            }
        }

        [Authorize(Roles = "Employee")]
        [HttpPost]
        public async Task<ActionResult<Guid>> CreateCategory([FromBody] CategoryDto categoryDto)
        {
            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = categoryDto.Name,
                Description = categoryDto.Description,
                ImageUrl = categoryDto.ImageUrl,
                IsDeleted = false
            };

            var id = await _categoryService.CreateCategoryAsync(category);
            return CreatedAtAction(nameof(GetCategoryById), new { id }, id);
        }

        [Authorize(Roles = "Employee")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] CategoryDto categoryDto)
        {
            try
            {
                var updatedCategory = new Category
                {
                    Id = id,
                    Name = categoryDto.Name,
                    Description = categoryDto.Description,
                    ImageUrl = categoryDto.ImageUrl
                };

                await _categoryService.UpdateCategoryAsync(id, updatedCategory);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Category not found.");
            }
        }

        [Authorize(Roles = "Employee")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            try
            {
                await _categoryService.DeleteCategoryAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Category not found.");
            }
        }

        // DRY: Common mapping logic
        private CategoryDto MapToDto(Category category)
        {
            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ImageUrl = category.ImageUrl
            };
        }
    }
}
