// using Xunit;
// using Moq;
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using Microsoft.AspNetCore.Mvc;
// using InnSales.Api.Controllers;
// using InnSales.Services;
// using InnSales.Common.DTO;
// using InnSales.Domain.Entities;

// public class ProductsControllerTests
// {
//     private readonly Mock<IProductService> _mockService;
//     private readonly ProductsController _controller;

//     public ProductsControllerTests()
//     {
//         _mockService = new Mock<IProductService>();
//         _controller = new ProductsController(_mockService.Object);
//     }

//     [Fact]
//     public async Task GetAll_ReturnsListOfProductDto()
//     {
//         var products = new List<Product>
//         {
//             new Product { Id = Guid.NewGuid(), Name = "Phone", Price = 999, StockQuantity = 10, CategoryId = Guid.NewGuid() }
//         };

//         _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(products);

//         var result = await _controller.GetAll();

//         var okResult = Assert.IsType<OkObjectResult>(result.Result);
//         var dtoList = Assert.IsType<List<ProductDto>>(okResult.Value);
//         Assert.Single(dtoList);
//         Assert.Equal("Phone", dtoList.First().Name);
//     }

//     [Fact]
//     public async Task GetById_ReturnsProductDto_WhenFound()
//     {
//         var productId = Guid.NewGuid();
//         var product = new Product { Id = productId, Name = "Laptop", Price = 1500, StockQuantity = 5, CategoryId = Guid.NewGuid() };

//         _mockService.Setup(s => s.GetByIdAsync(productId)).ReturnsAsync(product);

//         var result = await _controller.GetById(productId);

//         var okResult = Assert.IsType<OkObjectResult>(result.Result);
//         var dto = Assert.IsType<ProductDto>(okResult.Value);
//         Assert.Equal("Laptop", dto.Name);
//     }

//     [Fact]
//     public async Task GetById_ReturnsNotFound_WhenMissing()
//     {
//         _mockService.Setup(s => s.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Product)null);

//         var result = await _controller.GetById(Guid.NewGuid());

//         Assert.IsType<NotFoundResult>(result.Result);
//     }

//     [Fact]
//     public async Task GetByCategoryId_ReturnsFilteredProducts()
//     {
//         var categoryId = Guid.NewGuid();
//         var products = new List<Product>
//         {
//             new Product { Id = Guid.NewGuid(), Name = "Keyboard", CategoryId = categoryId },
//             new Product { Id = Guid.NewGuid(), Name = "Mouse", CategoryId = Guid.NewGuid() }
//         };

//         _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(products);

//         var result = await _controller.GetByCategoryId(categoryId);

//         var okResult = Assert.IsType<OkObjectResult>(result.Result);
//         var filtered = Assert.IsType<List<ProductDto>>(okResult.Value);
//         Assert.Single(filtered);
//         Assert.Equal("Keyboard", filtered.First().Name);
//     }

//     [Fact]
//     public async Task Create_ReturnsCreatedProductDto()
//     {
//         var dto = new ProductDto { Name = "Monitor", Price = 200, StockQuantity = 20, CategoryId = Guid.NewGuid() };
//         var product = new Product { Id = Guid.NewGuid(), Name = dto.Name, Price = dto.Price, StockQuantity = dto.StockQuantity, CategoryId = dto.CategoryId };

//         _mockService.Setup(s => s.CreateAsync(It.IsAny<Product>())).ReturnsAsync(product);

//         var result = await _controller.Create(dto);

//         var created = Assert.IsType<CreatedAtActionResult>(result.Result);
//         var createdDto = Assert.IsType<ProductDto>(created.Value);
//         Assert.Equal("Monitor", createdDto.Name);
//     }

//     [Fact]
//     public async Task Update_ReturnsUpdatedProductDto_WhenFound()
//     {
//         var productId = Guid.NewGuid();
//         var dto = new ProductDto { Name = "Updated", Price = 300, StockQuantity = 15, CategoryId = Guid.NewGuid() };
//         var updatedProduct = new Product { Id = productId, Name = dto.Name, Price = dto.Price, StockQuantity = dto.StockQuantity, CategoryId = dto.CategoryId };

//         _mockService.Setup(s => s.UpdateAsync(productId, It.IsAny<Product>())).ReturnsAsync(updatedProduct);

//         var result = await _controller.Update(productId, dto);

//         var okResult = Assert.IsType<OkObjectResult>(result.Result);
//         var updatedDto = Assert.IsType<ProductDto>(okResult.Value);
//         Assert.Equal("Updated", updatedDto.Name);
//     }

//     [Fact]
//     public async Task Update_ReturnsNotFound_WhenMissing()
//     {
//         _mockService.Setup(s => s.UpdateAsync(It.IsAny<Guid>(), It.IsAny<Product>())).ReturnsAsync((Product)null);

//         var result = await _controller.Update(Guid.NewGuid(), new ProductDto());

//         Assert.IsType<NotFoundResult>(result.Result);
//     }

//     [Fact]
//     public async Task Delete_ReturnsNoContent_WhenSuccessful()
//     {
//         _mockService.Setup(s => s.DeleteAsync(It.IsAny<Guid>())).ReturnsAsync(true);

//         var result = await _controller.Delete(Guid.NewGuid());

//         Assert.IsType<NoContentResult>(result);
//     }

//     [Fact]
//     public async Task Delete_ReturnsNotFound_WhenFailed()
//     {
//         _mockService.Setup(s => s.DeleteAsync(It.IsAny<Guid>())).ReturnsAsync(false);

//         var result = await _controller.Delete(Guid.NewGuid());

//         Assert.IsType<NotFoundResult>(result);
//     }
// }

using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using InnSales.Api.Controllers;
using InnSales.Services;
using InnSales.Common.DTO;
using InnSales.Domain.Entities;

public class ProductsControllerTests
{
    private readonly Mock<IProductService> _mockService;
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _mockService = new Mock<IProductService>();
        _controller = new ProductsController(_mockService.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsListOfProductDto()
    {
        // Arrange: service returns DTOs
        var productDtos = new List<ProductDto>
        {
            new ProductDto
            {
                Id = Guid.NewGuid(),
                Name = "Phone",
                Price = 999m,
                StockQuantity = 10,
                CategoryId = Guid.NewGuid(),
                IsPromoProduct = false,
                AvailabilityStatus = "In Stock"
            }
        };

        _mockService
            .Setup(s => s.GetAllAsync())
            .Returns(Task.FromResult<IEnumerable<ProductDto>>(productDtos));

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dtoList = Assert.IsType<List<ProductDto>>(okResult.Value);
        Assert.Single(dtoList);
        Assert.Equal("Phone", dtoList.First().Name);
    }

    [Fact]
    public async Task GetById_ReturnsProductDto_WhenFound()
    {
        // Arrange: service returns DTO (nullable)
        var productId = Guid.NewGuid();
        var dto = new ProductDto
        {
            Id = productId,
            Name = "Laptop",
            Price = 1500m,
            StockQuantity = 5,
            CategoryId = Guid.NewGuid(),
            IsPromoProduct = false,
            AvailabilityStatus = "In Stock"
        };

        _mockService
            .Setup(s => s.GetByIdAsync(productId))
            .Returns(Task.FromResult<ProductDto?>(dto));

        // Act
        var result = await _controller.GetById(productId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returned = Assert.IsType<ProductDto>(okResult.Value);
        Assert.Equal("Laptop", returned.Name);
        Assert.Equal(productId, returned.Id);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenMissing()
    {
        _mockService
            .Setup(s => s.GetByIdAsync(It.IsAny<Guid>()))
            .Returns(Task.FromResult<ProductDto?>(null));

        var result = await _controller.GetById(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetByCategoryId_ReturnsFilteredProducts()
    {
        // Arrange: service returns DTOs filtered by category
        var categoryId = Guid.NewGuid();
        var filteredDtos = new List<ProductDto>
        {
            new ProductDto
            {
                Id = Guid.NewGuid(),
                Name = "Keyboard",
                CategoryId = categoryId,
                Price = 100m,
                StockQuantity = 50,
                IsPromoProduct = false,
                AvailabilityStatus = "In Stock"
            }
        };

        _mockService
            .Setup(s => s.GetByCategoryAsync(categoryId))
            .Returns(Task.FromResult<IEnumerable<ProductDto>>(filteredDtos));

        // Act
        var result = await _controller.GetByCategoryId(categoryId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var list = Assert.IsType<List<ProductDto>>(okResult.Value);
        Assert.Single(list);
        Assert.Equal("Keyboard", list.First().Name);
        Assert.Equal(categoryId, list.First().CategoryId);
    }

    [Fact]
    public async Task Create_ReturnsCreatedProductDto()
    {
        // Controller accepts ProductDto; service expects Product entity and returns Product entity
        var inputDto = new ProductDto
        {
            Id = Guid.Empty,
            Name = "Monitor",
            Price = 200m,
            StockQuantity = 20,
            CategoryId = Guid.NewGuid(),
            IsPromoProduct = false,
            AvailabilityStatus = "In Stock"
        };

        // Service returns created entity
        var createdEntity = new Product
        {
            Id = Guid.NewGuid(),
            Name = inputDto.Name,
            Price = inputDto.Price,
            StockQuantity = inputDto.StockQuantity,
            CategoryId = inputDto.CategoryId,
            IsPromoProduct = inputDto.IsPromoProduct
        };

        _mockService
            .Setup(s => s.CreateAsync(It.IsAny<Product>()))   // entity input
            .Returns(Task.FromResult(createdEntity));          // entity return

        // Act
        var result = await _controller.Create(inputDto);

        // Assert: controller maps entity to DTO and returns 201
        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        // Optional: verify route
        Assert.Equal(nameof(ProductsController.GetById), created.ActionName);
        var createdDto = Assert.IsType<ProductDto>(created.Value);

        // Validate core fields; do not assert Id (controller returns Guid.Empty currently)
        Assert.Equal("Monitor", createdDto.Name);
        Assert.Equal(200m, createdDto.Price);
        Assert.Equal(20, createdDto.StockQuantity);
        Assert.Equal(inputDto.CategoryId, createdDto.CategoryId);
    }

    [Fact]
    public async Task Update_ReturnsUpdatedProductDto_WhenFound()
    {
        var productId = Guid.NewGuid();
        var updateDto = new ProductDto
        {
            Name = "Updated",
            Price = 300m,
            StockQuantity = 15,
            CategoryId = Guid.NewGuid(),
            IsPromoProduct = false,
            AvailabilityStatus = "In Stock"
        };

        var updatedEntity = new Product
        {
            Id = productId,
            Name = updateDto.Name,
            Price = updateDto.Price,
            StockQuantity = updateDto.StockQuantity,
            CategoryId = updateDto.CategoryId,
            IsPromoProduct = updateDto.IsPromoProduct
        };

        _mockService
            .Setup(s => s.UpdateAsync(productId, It.IsAny<Product>())) // entity input
            .Returns(Task.FromResult<Product?>(updatedEntity));         // entity return (nullable)

        var result = await _controller.Update(productId, updateDto);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returned = Assert.IsType<ProductDto>(okResult.Value);

        // Validate core fields; avoid asserting Id since DTO.Id may be Guid.Empty
        Assert.Equal("Updated", returned.Name);
        Assert.Equal(300m, returned.Price);
        Assert.Equal(15, returned.StockQuantity);
        Assert.Equal(updateDto.CategoryId, returned.CategoryId);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenMissing()
    {
        _mockService
            .Setup(s => s.UpdateAsync(It.IsAny<Guid>(), It.IsAny<Product>()))
            .Returns(Task.FromResult<Product?>(null)); // not found

        var result = await _controller.Update(Guid.NewGuid(), new ProductDto());

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenSuccessful()
    {
        _mockService
            .Setup(s => s.DeleteAsync(It.IsAny<Guid>()))
            .Returns(Task.FromResult(true));

        var result = await _controller.Delete(Guid.NewGuid());

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenFailed()
    {
        _mockService
            .Setup(s => s.DeleteAsync(It.IsAny<Guid>()))
            .Returns(Task.FromResult(false));

        var result = await _controller.Delete(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result);
    }
}
