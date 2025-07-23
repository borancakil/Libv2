using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LibraryApp.Application.Interfaces;
using LibraryApp.Application.DTOs.Book;
using LibraryApp.Application.Exceptions;
using LibraryApp.Domain.Interfaces;

namespace LibraryApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _bookService;
        private readonly ILogger<BooksController> _logger;
        private readonly IAuthorRepository _authorRepository;
        private readonly IPublisherRepository _publisherRepository;

        public BooksController(IBookService bookService, ILogger<BooksController> logger, IAuthorRepository authorRepository, IPublisherRepository publisherRepository)
        {
            _bookService = bookService ?? throw new ArgumentNullException(nameof(bookService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _authorRepository = authorRepository ?? throw new ArgumentNullException(nameof(authorRepository));
            _publisherRepository = publisherRepository ?? throw new ArgumentNullException(nameof(publisherRepository));
        }

        /// <summary>
        /// Creates a new book
        /// </summary>
        /// <param name="dto">Book creation data</param>
        /// <returns>Created book information</returns>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBookDto dto)
        {
            _logger.LogInformation("🔍 CREATE BOOK DEBUG - Step 1: Method entered");
            _logger.LogInformation("🔍 CREATE BOOK DEBUG - DTO: {@Dto}", dto);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("🔍 CREATE BOOK DEBUG - Step 2: ModelState is invalid: {@ModelState}", ModelState);
                return BadRequest(ModelState);
            }

            _logger.LogInformation("🔍 CREATE BOOK DEBUG - Step 3: ModelState is valid");

            try
            {
                _logger.LogInformation("🔍 CREATE BOOK DEBUG - Step 4: Starting book creation for title: {Title}", dto.Title);
                
                var bookId = await _bookService.AddBookAsync(dto);
                
                _logger.LogInformation("🔍 CREATE BOOK DEBUG - Step 5: Book created successfully with ID: {BookId}", bookId);
                
                // Simple response without GetByIdAsync to avoid navigation property issues
                var simpleResponse = new 
                {
                    BookId = bookId,
                    Title = dto.Title,
                    PublicationYear = dto.PublicationYear,
                    AuthorId = dto.AuthorId,
                    PublisherId = dto.PublisherId,
                    IsAvailable = true,
                    Message = "Book created successfully"
                };
                
                _logger.LogInformation("🔍 CREATE BOOK DEBUG - Step 6: Simple response created: {@Response}", simpleResponse);
                
                // Return 201 Created with location header and simple object
                var result = CreatedAtAction(
                    nameof(GetById), 
                    new { id = bookId }, 
                    simpleResponse
                );
                
                _logger.LogInformation("🔍 CREATE BOOK DEBUG - Step 7: CreatedAtAction result created successfully");
                return result;
            }
            catch (LibraryApp.Application.Exceptions.ValidationException ex)
            {
                _logger.LogWarning("🔥 CREATE BOOK DEBUG - CAUGHT ValidationException: {Message}", ex.Message);
                _logger.LogWarning("🔥 CREATE BOOK DEBUG - ValidationException Errors: {@Errors}", ex.Errors);
                return BadRequest(new { message = ex.Message, errors = ex.Errors });
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError("🔥 CREATE BOOK DEBUG - CAUGHT ArgumentNullException: {Message}", ex.Message);
                _logger.LogError("🔥 CREATE BOOK DEBUG - ArgumentNullException StackTrace: {StackTrace}", ex.StackTrace);
                return StatusCode(500, new { message = "ArgumentNullException: " + ex.Message, exceptionType = "ArgumentNullException" });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError("🔥 CREATE BOOK DEBUG - CAUGHT ArgumentException: {Message}", ex.Message);
                _logger.LogError("🔥 CREATE BOOK DEBUG - ArgumentException StackTrace: {StackTrace}", ex.StackTrace);
                return StatusCode(500, new { message = "ArgumentException: " + ex.Message, exceptionType = "ArgumentException" });
            }
            catch (Exception ex)
            {
                _logger.LogError("🔥 CREATE BOOK DEBUG - CAUGHT Generic Exception: {Message}", ex.Message);
                _logger.LogError("🔥 CREATE BOOK DEBUG - Exception Type: {ExceptionType}", ex.GetType().Name);
                _logger.LogError("🔥 CREATE BOOK DEBUG - Exception StackTrace: {StackTrace}", ex.StackTrace);
                _logger.LogError("🔥 CREATE BOOK DEBUG - Inner Exception: {InnerException}", ex.InnerException?.Message);
                return StatusCode(500, new { 
                    message = "An error occurred while creating the book", 
                    exceptionType = ex.GetType().Name,
                    exceptionMessage = ex.Message,
                    innerException = ex.InnerException?.Message,
                    step = "Unknown - check logs"
                });
            }
        }

        /// <summary>
        /// Gets all books
        /// </summary>
        /// <param name="includeUnavailable">Whether to include unavailable books</param>
        /// <returns>List of books</returns>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool includeUnavailable = true)
        {
            try
            {
                var books = await _bookService.GetAllBooksAsync(includeUnavailable);
                return Ok(books);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving books");
                return StatusCode(500, new { message = "An error occurred while retrieving books" });
            }
        }

        /// <summary>
        /// Gets a book by ID
        /// </summary>
        /// <param name="id">Book ID</param>
        /// <returns>Book details</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { message = "Invalid book ID" });
            }

            try
            {
                var book = await _bookService.GetByIdAsync(id);

                if (book == null)
                {
                    return NotFound(new { message = $"Book with ID {id} not found" });
                }

                return Ok(book);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving book with ID {BookId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving the book" });
            }
        }

        /// <summary>
        /// Updates an existing book
        /// </summary>
        /// <param name="id">Book ID</param>
        /// <param name="dto">Updated book data</param>
        /// <returns>No content on success</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateBookDto dto)
        {
            if (id <= 0)
            {
                return BadRequest(new { message = "Invalid book ID" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _bookService.UpdateBookAsync(id, dto);
                return NoContent(); // 204 No Content is standard for successful updates
            }
            catch (BookNotFoundException ex)
            {
                _logger.LogWarning("Book not found for update: {BookId}", ex.BookId);
                return NotFound(new { message = ex.Message });
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning("Validation failed for book update: {BookId}, {Errors}", id, ex.Errors);
                return BadRequest(new { message = ex.Message, errors = ex.Errors });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating book with ID {BookId}", id);
                return StatusCode(500, new { message = "An error occurred while updating the book" });
            }
        }

        /// <summary>
        /// Deletes a book
        /// </summary>
        /// <param name="id">Book ID</param>
        /// <returns>No content on success</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { message = "Invalid book ID" });
            }

            try
            {
                await _bookService.DeleteBookAsync(id);
                return NoContent(); // 204 No Content is standard for successful deletions
            }
            catch (BookNotFoundException ex)
            {
                _logger.LogWarning("Book not found for deletion: {BookId}", ex.BookId);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Cannot delete book: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting book with ID {BookId}", id);
                return StatusCode(500, new { message = "An error occurred while deleting the book" });
            }
        }

        /// <summary>
        /// Borrows a book
        /// </summary>
        /// <param name="id">Book ID</param>
        /// <param name="borrowDto">Borrowing information</param>
        /// <returns>Success message</returns>
        [HttpPost("{id}/borrow")]
        public async Task<IActionResult> Borrow(int id, [FromBody] BorrowBookDto borrowDto)
        {
            if (id <= 0)
            {
                return BadRequest(new { message = "Invalid book ID" });
            }

            if (id != borrowDto.BookId)
            {
                return BadRequest(new { message = "ID in URL does not match BookId in request body" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _bookService.BorrowBookAsync(borrowDto);
                return Ok(new { message = "Book successfully borrowed", bookId = id });
            }
            catch (BookNotFoundException ex)
            {
                _logger.LogWarning("Book not found for borrowing: {BookId}", ex.BookId);
                return NotFound(new { message = ex.Message });
            }
            catch (BookNotAvailableException ex)
            {
                _logger.LogWarning("Book not available for borrowing: {BookId} - {Title}", ex.BookId, ex.BookTitle);
                return Conflict(new { message = ex.Message }); // 409 Conflict for business rule violations
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning("Validation failed for borrowing: {Errors}", ex.Errors);
                return BadRequest(new { message = ex.Message, errors = ex.Errors });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error borrowing book with ID {BookId}", id);
                return StatusCode(500, new { message = "An error occurred while borrowing the book" });
            }
        }

        /// <summary>
        /// Returns a borrowed book
        /// </summary>
        /// <param name="id">Book ID</param>
        /// <param name="userId">User ID who is returning the book</param>
        /// <returns>Success message</returns>
        [HttpPost("{id}/return")]
        public async Task<IActionResult> Return(int id, [FromBody] int userId)
        {
            if (id <= 0)
            {
                return BadRequest(new { message = "Invalid book ID" });
            }

            if (userId <= 0)
            {
                return BadRequest(new { message = "Invalid user ID" });
            }

            try
            {
                await _bookService.ReturnBookAsync(id, userId);
                return Ok(new { message = "Book successfully returned", bookId = id });
            }
            catch (BookNotFoundException ex)
            {
                _logger.LogWarning("Book not found for return: {BookId}", ex.BookId);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Cannot return book: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error returning book with ID {BookId}", id);
                return StatusCode(500, new { message = "An error occurred while returning the book" });
            }
        }

        /// <summary>
        /// Checks if a book exists
        /// </summary>
        /// <param name="id">Book ID</param>
        /// <returns>Existence status</returns>
        [HttpHead("{id}")]
        public async Task<IActionResult> Exists(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            try
            {
                var exists = await _bookService.BookExistsAsync(id);
                return exists ? Ok() : NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking book existence with ID {BookId}", id);
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Check if author exists
        /// </summary>
        /// <param name="id">Author ID</param>
        /// <returns>True if exists</returns>
        [HttpGet("authors/{id}/exists")]
        public async Task<IActionResult> AuthorExists(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Invalid author ID" });

                var exists = await _authorRepository.ExistsAsync(id);
                return Ok(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if author exists with ID: {AuthorId}", id);
                return StatusCode(500, new { message = "An error occurred while checking author" });
            }
        }

        /// <summary>
        /// Check if publisher exists
        /// </summary>
        /// <param name="id">Publisher ID</param>
        /// <returns>True if exists</returns>
        [HttpGet("publishers/{id}/exists")]
        public async Task<IActionResult> PublisherExists(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Invalid publisher ID" });

                var exists = await _publisherRepository.ExistsAsync(id);
                return Ok(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if publisher exists with ID: {PublisherId}", id);
                return StatusCode(500, new { message = "An error occurred while checking publisher" });
            }
        }
    }
}