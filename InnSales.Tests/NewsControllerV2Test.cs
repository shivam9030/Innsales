using Xunit;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using InnSales.Api.Controllers;
using InnSales.Domain.Entities;
using InnSales.Services;

using System;
using System.Linq;
using AutoMapper;

public class NewsV2ControllerTests
{
    private readonly Mock<INewsService> _mockNewsService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly NewsV2Controller _controller;

    public NewsV2ControllerTests()
    {
        _mockNewsService = new Mock<INewsService>();
        _mockMapper = new Mock<IMapper>();

        _controller = new NewsV2Controller(_mockNewsService.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task GetAllNews_ReturnsListOfNewsDtoV2()
    {
        var newsList = new List<News>
        {
            new News { Id = 1, Title = "Title", Description = "Desc", PublishedBy = "Author", PublishedDate = DateTime.UtcNow, EditedBy = "Editor", EditedDate = DateTime.UtcNow, CreatedBy = "Creator", CreatedDate = DateTime.UtcNow, Category = "General", SourceUrl = "http://example.com" }
        };

        var dtoList = new List<NewsDtoV2>
        {
            new NewsDtoV2 { Id = 1, Title = "Title" }
        };

        _mockNewsService.Setup(s => s.GetAllNewsAsync()).ReturnsAsync(newsList);
        _mockMapper.Setup(m => m.Map<List<NewsDtoV2>>(newsList)).Returns(dtoList);

        var result = await _controller.GetAllNews();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<List<NewsDtoV2>>(okResult.Value);
        Assert.Single(returnValue);
        Assert.Equal("Title", returnValue.First().Title);
    }

    [Fact]
    public async Task GetNewsById_ReturnsNewsDtoV2_WhenFound()
    {
        var news = new News { Id = 1, Title = "Title" };
        var dto = new NewsDtoV2 { Id = 1, Title = "Title" };

        _mockNewsService.Setup(s => s.GetNewsByIdAsync(1)).ReturnsAsync(news);
        _mockMapper.Setup(m => m.Map<NewsDtoV2>(news)).Returns(dto);

        var result = await _controller.GetNewsById(1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnDto = Assert.IsType<NewsDtoV2>(okResult.Value);
        Assert.Equal(1, returnDto.Id);
    }

    [Fact]
    public async Task GetNewsById_ReturnsNotFound_WhenNotFound()
    {
        _mockNewsService.Setup(s => s.GetNewsByIdAsync(1)).ReturnsAsync((News)null);

        var result = await _controller.GetNewsById(1);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task CreateNews_ReturnsCreatedAtActionResult()
    {
        var dto = new NewsDtoV2 { Title = "Title" };
        var entity = new News { Id = 1, Title = "Title" };

        _mockMapper.Setup(m => m.Map<News>(dto)).Returns(entity);
        _mockNewsService.Setup(s => s.CreateNewsAsync(entity)).ReturnsAsync(1);

        var result = await _controller.CreateNews(dto);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(1, createdResult.Value);
    }

    [Fact]
    public async Task UpdateNews_ReturnsNoContent_WhenSuccessful()
    {
        var dto = new NewsDtoV2 { Title = "Updated" };
        var existingNews = new News { Id = 1 };

        _mockNewsService.Setup(s => s.GetNewsByIdAsync(1)).ReturnsAsync(existingNews);
        _mockMapper.Setup(m => m.Map<News>(dto)).Returns(existingNews);
        _mockNewsService.Setup(s => s.UpdateNewsAsync(1, existingNews)).Returns(Task.CompletedTask);

        var result = await _controller.UpdateNews(1, dto);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task UpdateNews_ReturnsNotFound_WhenNewsDoesNotExist()
    {
        _mockNewsService.Setup(s => s.GetNewsByIdAsync(1)).ReturnsAsync((News)null);

        var dto = new NewsDtoV2();

        var result = await _controller.UpdateNews(1, dto);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteNews_ReturnsNoContent_WhenSuccessful()
    {
        var existingNews = new News { Id = 1 };

        _mockNewsService.Setup(s => s.GetNewsByIdAsync(1)).ReturnsAsync(existingNews);
        _mockNewsService.Setup(s => s.DeleteNewsAsync(1)).Returns(Task.CompletedTask);

        var result = await _controller.DeleteNews(1);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteNews_ReturnsNotFound_WhenNewsDoesNotExist()
    {
        _mockNewsService.Setup(s => s.GetNewsByIdAsync(1)).ReturnsAsync((News)null);

        var result = await _controller.DeleteNews(1);

        Assert.IsType<NotFoundResult>(result);
    }
}