using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LibraryApp.Application.DTOs.Book;
using LibraryApp.Application.Interfaces;

namespace LibraryApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _bookService;
        private readonly ILogger<BooksController> _logger;

        public BooksController(IBookService bookService, ILogger<BooksController> logger)
        {
            _bookService = bookService;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddBook([FromBody] CreateBookDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var bookId = await _bookService.AddBookAsync(dto);
            var book = await _bookService.GetByIdAsync(bookId);
            return CreatedAtAction(nameof(GetById), new { id = bookId }, book);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBooks([FromQuery] string? filter = null, [FromQuery] bool includeUnavailable = true)
        {
            var books = await _bookService.GetAllBooksAsync(filter, includeUnavailable);
            return Ok(books);
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetAllBooksForList([FromQuery] string? filter = null, [FromQuery] bool includeUnavailable = true)
        {
            var books = await _bookService.GetAllBooksForListAsync(filter, includeUnavailable);
            return Ok(books);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0) return BadRequest(new { message = "Invalid book ID" });
            var book = await _bookService.GetByIdAsync(id);
            if (book == null) return NotFound(new { message = $"Book with ID {id} not found" });
            return Ok(book);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateBook(int id, [FromBody] UpdateBookDto dto)
        {
            if (id <= 0) return BadRequest(new { message = "Invalid book ID" });
            if (!ModelState.IsValid) return BadRequest(ModelState);
            await _bookService.UpdateBookAsync(id, dto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            if (id <= 0) return BadRequest(new { message = "Invalid book ID" });
            await _bookService.DeleteBookAsync(id);
            return NoContent();
        }

        [HttpHead("{id}")]
        public async Task<IActionResult> BookExists(int id)
        {
            if (id <= 0) return BadRequest();
            var exists = await _bookService.BookExistsAsync(id);
            return exists ? Ok() : NotFound();
        }

        [HttpGet("{id}/cover")]
        public async Task<IActionResult> GetBookCover(int id)
        {
            if (id <= 0) return BadRequest(new { message = "Invalid book ID" });
            var (content, contentType, fileName) = await _bookService.GetBookCoverAsync(id);
            if (content == null) return NotFound(new { message = "Cover image not found" });
            return File(content, contentType, fileName);
        }

        [HttpDelete("{id}/cover")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteBookCover(int id)
        {
            if (id <= 0) return BadRequest(new { message = "Invalid book ID" });
            await _bookService.DeleteBookCoverAsync(id);
            return NoContent();
        }

        [HttpPost("{id}/borrow")]
        [Authorize]
        public async Task<IActionResult> BorrowBook(int id, [FromBody] BorrowBookDto dto)
        {
            if (id <= 0) return BadRequest(new { message = "Invalid book ID" });
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            // DTO'dan gelen bilgileri kullan
            if (dto.BookId != id)
                return BadRequest(new { message = "Book ID mismatch" });
            
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized(new { message = "User ID not found in token" });
            
            if (dto.UserId != userId.Value)
                return BadRequest(new { message = "User ID mismatch" });
            
            await _bookService.BorrowBookAsync(id, userId.Value);
            return Ok(new { message = "Book borrowed successfully" });
        }

        [HttpPost("{id}/return")]
        [Authorize]
        public async Task<IActionResult> ReturnBook(int id, [FromBody] ReturnBookDto dto)
        {
            if (id <= 0) return BadRequest(new { message = "Invalid book ID" });
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized(new { message = "User ID not found in token" });
            await _bookService.ReturnBookAsync(id, userId.Value);
            return Ok(new { message = "Book returned successfully" });
        }

        [HttpGet("authors/{id}/exists")]
        public async Task<IActionResult> AuthorExists(int id)
        {
            if (id <= 0) return BadRequest(new { message = "Invalid author ID" });
            var exists = await _bookService.AuthorExistsAsync(id);
            return Ok(new { exists });
        }

        [HttpGet("authors/{id}/books")]
        public async Task<IActionResult> GetAuthorBooks(int id)
        {
            if (id <= 0) return BadRequest(new { message = "Invalid author ID" });
            var books = await _bookService.GetAuthorBooksAsync(id);
            return Ok(books);
        }

        [HttpGet("publishers/{id}/exists")]
        public async Task<IActionResult> PublisherExists(int id)
        {
            if (id <= 0) return BadRequest(new { message = "Invalid publisher ID" });
            var exists = await _bookService.PublisherExistsAsync(id);
            return Ok(new { exists });
        }

        [HttpGet("publishers/{id}/books")]
        public async Task<IActionResult> GetPublisherBooks(int id)
        {
            if (id <= 0) return BadRequest(new { message = "Invalid publisher ID" });
            var books = await _bookService.GetPublisherBooksAsync(id);
            return Ok(books);
        }

        [HttpGet("{id}/borrowed-by/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetBookBorrowedByUser(int id, int userId)
        {
            if (id <= 0) return BadRequest(new { message = "Invalid book ID" });
            if (userId <= 0) return BadRequest(new { message = "Invalid user ID" });
            var loan = await _bookService.GetBookBorrowedByUserAsync(id, userId);
            return Ok(loan);
        }

        [HttpGet("{id}/status-for-user")]
        [Authorize]
        public async Task<IActionResult> GetBookStatusForUser(int id)
        {
            if (id <= 0) return BadRequest(new { message = "Invalid book ID" });
            
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized(new { message = "User ID not found in token" });
                
            var status = await _bookService.GetBookStatusForUserAsync(id, userId.Value);
            return Ok(status);
        }

        [HttpPost("status-for-user-batch")]
        [Authorize]
        public async Task<IActionResult> GetBookStatusForUserBatch([FromBody] int[] bookIds)
        {
            if (bookIds == null || bookIds.Length == 0)
                return BadRequest(new { message = "Book IDs are required" });
            
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized(new { message = "User ID not found in token" });
                
            var statuses = await _bookService.GetBookStatusForUserBatchAsync(bookIds, userId.Value);
            return Ok(statuses);
        }

        [HttpGet("{id}/favorite-count")]
        public async Task<IActionResult> GetBookFavoriteCount(int id)
        {
            if (id <= 0) return BadRequest(new { message = "Invalid book ID" });
            var count = await _bookService.GetBookFavoriteCountAsync(id);
            return Ok(new { count });
        }

        [HttpGet("{id}/favorited-by/{userId}")]
        [Authorize]
        public async Task<IActionResult> IsBookFavoritedByUser(int id, int userId)
        {
            if (id <= 0) return BadRequest(new { message = "Invalid book ID" });
            if (userId <= 0) return BadRequest(new { message = "Invalid user ID" });
            var isFavorited = await _bookService.IsBookFavoritedByUserAsync(id, userId);
            return Ok(new { isFavorited });
        }

        [HttpGet("{id}/favorited-by-users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsersWhoFavoritedBook(int id)
        {
            if (id <= 0) return BadRequest(new { message = "Invalid book ID" });
            var users = await _bookService.GetUsersWhoFavoritedBookAsync(id);
            return Ok(users);
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("userId");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return null;
        }
    }
}