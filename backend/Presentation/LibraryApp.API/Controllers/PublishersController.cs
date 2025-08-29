using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LibraryApp.Application.DTOs.Publisher;
using LibraryApp.Application.Interfaces;

namespace LibraryApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PublishersController : ControllerBase
    {
        private readonly IPublisherService _publisherService;
        private readonly ILogger<PublishersController> _logger;

        public PublishersController(IPublisherService publisherService, ILogger<PublishersController> logger)
        {
            _publisherService = publisherService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPublishers([FromQuery] string? filter = null)
        {
            var publishers = await _publisherService.GetAllPublishersAsync(filter);
            return Ok(publishers);
        }

        [HttpGet("details")]
        public async Task<IActionResult> GetPublishersWithDetails([FromQuery] string? filter = null)
        {
            var publishers = await _publisherService.GetAllPublishersAsync(filter);
            return Ok(publishers);
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetAllPublishersForList([FromQuery] string? filter = null)
        {
            var publishers = await _publisherService.GetAllPublishersForListAsync(filter);
            return Ok(publishers);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddPublisher([FromBody] CreatePublisherDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var publisher = await _publisherService.AddPublisherAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = publisher.PublisherId }, publisher);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0) return BadRequest(new { message = "Invalid publisher ID" });
            var publisher = await _publisherService.GetByIdAsync(id);
            if (publisher == null) return NotFound(new { message = $"Publisher with ID {id} not found" });
            return Ok(publisher);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdatePublisher(int id, [FromBody] UpdatePublisherDto dto)
        {
            if (id <= 0) return BadRequest(new { message = "Invalid publisher ID" });
            if (!ModelState.IsValid) return BadRequest(ModelState);
            await _publisherService.UpdatePublisherAsync(id, dto);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePublisher(int id)
        {
            if (id <= 0) return BadRequest(new { message = "Invalid publisher ID" });
            await _publisherService.DeletePublisherAsync(id);
            return NoContent();
        }

        [HttpGet("{id:int}/books")]
        public async Task<IActionResult> GetPublisherBooks(int id)
        {
            if (id <= 0) return BadRequest(new { message = "Invalid publisher ID" });
            var books = await _publisherService.GetPublisherBooksAsync(id);
            return Ok(books);
        }

        [HttpGet("{id:int}/book-count")]
        public async Task<IActionResult> GetPublisherBookCount(int id)
        {
            if (id <= 0) return BadRequest(new { message = "Invalid publisher ID" });
            var count = await _publisherService.GetPublisherBookCountAsync(id);
            return Ok(new { count });
        }

        [HttpGet("{id:int}/logo-image")]
        public async Task<IActionResult> GetPublisherLogoImage(int id)
        {
            if (id <= 0) return BadRequest(new { message = "Invalid publisher ID" });
            var (content, contentType, fileName) = await _publisherService.GetPublisherLogoImageAsync(id);
            if (content == null) return NotFound(new { message = "Logo image not found" });
            return File(content, contentType, fileName);
        }

        [HttpDelete("{id:int}/logo-image")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePublisherLogoImage(int id)
        {
            if (id <= 0) return BadRequest(new { message = "Invalid publisher ID" });
            await _publisherService.DeletePublisherLogoImageAsync(id);
            return NoContent();
        }
    }
}