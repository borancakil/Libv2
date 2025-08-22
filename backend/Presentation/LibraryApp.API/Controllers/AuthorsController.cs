using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LibraryApp.Application.DTOs.Author;
using LibraryApp.Application.Interfaces;

namespace LibraryApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthorsController : ControllerBase
    {
        private readonly IAuthorService _authorService;
        private readonly ILogger<AuthorsController> _logger;

        public AuthorsController(IAuthorService authorService, ILogger<AuthorsController> logger)
        {
            _authorService = authorService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAuthors([FromQuery] string? filter = null)
        {
            var authors = await _authorService.GetAllAuthorsAsync(filter);
            return Ok(authors);
        }

        [HttpGet("details")]
        public async Task<IActionResult> GetAuthorsWithDetails([FromQuery] string? filter = null)
        {
            var authors = await _authorService.GetAllAuthorsAsync(filter);
            return Ok(authors);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddAuthor([FromBody] CreateAuthorDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var author = await _authorService.AddAuthorAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = author.AuthorId }, author);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0) return BadRequest(new { message = "Invalid author ID" });
            var author = await _authorService.GetByIdAsync(id);
            if (author == null) return NotFound(new { message = $"Author with ID {id} not found" });
            return Ok(author);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateAuthor(int id, [FromBody] UpdateAuthorDto dto)
        {
            if (id <= 0) return BadRequest(new { message = "Invalid author ID" });
            if (!ModelState.IsValid) return BadRequest(ModelState);
            await _authorService.UpdateAuthorAsync(id, dto);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAuthor(int id)
        {
            if (id <= 0) return BadRequest(new { message = "Invalid author ID" });
            await _authorService.DeleteAuthorAsync(id);
            return NoContent();
        }

        [HttpGet("{id:int}/books")]
        public async Task<IActionResult> GetAuthorBooks(int id)
        {
            if (id <= 0) return BadRequest(new { message = "Invalid author ID" });
            var books = await _authorService.GetAuthorBooksAsync(id);
            return Ok(books);
        }

        [HttpGet("{id:int}/book-count")]
        public async Task<IActionResult> GetAuthorBookCount(int id)
        {
            if (id <= 0) return BadRequest(new { message = "Invalid author ID" });
            var count = await _authorService.GetAuthorBookCountAsync(id);
            return Ok(new { count });
        }

        [HttpGet("{id:int}/profile-image")]
        public async Task<IActionResult> GetAuthorProfileImage(int id)
        {
            if (id <= 0) return BadRequest(new { message = "Invalid author ID" });
            var (content, contentType, fileName) = await _authorService.GetAuthorProfileImageAsync(id);
            if (content == null) return NotFound(new { message = "Profile image not found" });
            return File(content, contentType, fileName);
        }

        [HttpDelete("{id:int}/profile-image")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAuthorProfileImage(int id)
        {
            if (id <= 0) return BadRequest(new { message = "Invalid author ID" });
            await _authorService.DeleteAuthorProfileImageAsync(id);
            return NoContent();
        }
    }
}