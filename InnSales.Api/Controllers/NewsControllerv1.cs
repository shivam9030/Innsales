

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InnSales.Domain.Entities;
using InnSales.Services;
using InnSales.Common.DTO;
using AutoMapper;
namespace InnSales.Api.Controllers
{
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/news")]
    public class NewsV1Controller : ControllerBase
    {
        private readonly INewsService _newsService;
        private readonly IMapper _mapper;

        public NewsV1Controller(INewsService newsService,IMapper mapper)
        {
            _newsService = newsService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<NewsDtoV1>>> GetAllNews()
        {
            var newsList = await _newsService.GetAllNewsAsync();
            var dtoList =_mapper.Map<List<NewsDtoV1>>(newsList);    

            return Ok(dtoList);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<NewsDtoV1>> GetNewsById(int id)
        {
            var news = await _newsService.GetNewsByIdAsync(id);
            if (news == null)
                return NotFound();

            var dto = _mapper.Map<NewsDtoV1>(news);

            return Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult<int>> CreateNews([FromBody] NewsDtoV1 dto)
        {
            var news = _mapper.Map<News>(dto);

            var newsId = await _newsService.CreateNewsAsync(news);
            return CreatedAtAction(nameof(GetNewsById), new { id = newsId }, newsId);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNews(int id, [FromBody] NewsDtoV1 dto)
        {
            var existingNews = await _newsService.GetNewsByIdAsync(id);
            if (existingNews == null)
                return NotFound();

                _mapper.Map(dto, existingNews);

            await _newsService.UpdateNewsAsync(id, existingNews);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNews(int id)
        {
            var existingNews = await _newsService.GetNewsByIdAsync(id);
            if (existingNews == null)
                return NotFound();

            await _newsService.DeleteNewsAsync(id);
            return NoContent();
        }
    }
}
