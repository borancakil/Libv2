using Microsoft.AspNetCore.Mvc;
using LibraryApp.Application.DTOs.Author;
using LibraryApp.Application.Services;
using LibraryApp.Application.Exceptions;
using FluentValidation;

namespace LibraryApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthorsController : ControllerBase
    {
        private readonly AuthorService _authorService;
        private readonly IValidator<CreateAuthorDto> _createValidator;
        private readonly IValidator<UpdateAuthorDto> _updateValidator;
        private readonly ILogger<AuthorsController> _logger;

        public AuthorsController(
            AuthorService authorService,
            IValidator<CreateAuthorDto> createValidator,
            IValidator<UpdateAuthorDto> updateValidator,
            ILogger<AuthorsController> logger)
        {
            _authorService = authorService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _logger = logger;
        }

        /// <summary>
        /// Get all authors
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AuthorDto>>> GetAll()
        {
            try
            {
                _logger.LogInformation("Getting all authors");
                var authors = await _authorService.GetAllAuthorsAsync();
                return Ok(authors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all authors");
                throw;
            }
        }

        /// <summary>
        /// Get author by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<AuthorDto>> GetById(int id)
        {
            try
            {
                _logger.LogInformation("Getting author with ID: {AuthorId}", id);
                var author = await _authorService.GetAuthorByIdAsync(id);
                return Ok(author);
            }
            catch (AuthorNotFoundException ex)
            {
                _logger.LogWarning("Author not found: {AuthorId}", id);
                return NotFound(new { message = ex.Message, authorId = ex.AuthorId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting author with ID: {AuthorId}", id);
                throw;
            }
        }

        /// <summary>
        /// Create a new author
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<AuthorDto>> Create([FromBody] CreateAuthorDto createAuthorDto)
        {
            try
            {
                _logger.LogInformation("Creating new author: {AuthorName}", createAuthorDto.Name);

                // Validate
                var validationResult = await _createValidator.ValidateAsync(createAuthorDto);
                if (!validationResult.IsValid)
                {
                    foreach (var error in validationResult.Errors)
                    {
                        ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                    }
                    return BadRequest(ModelState);
                }

                var createdAuthor = await _authorService.CreateAuthorAsync(createAuthorDto);
                _logger.LogInformation("Author created successfully with ID: {AuthorId}", createdAuthor.AuthorId);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = createdAuthor.AuthorId },
                    createdAuthor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating author: {AuthorName}", createAuthorDto.Name);
                throw;
            }
        }

        /// <summary>
        /// Update an existing author
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAuthorDto updateAuthorDto)
        {
            try
            {
                _logger.LogInformation("Updating author with ID: {AuthorId}", id);

                // Validate
                var validationResult = await _updateValidator.ValidateAsync(updateAuthorDto);
                if (!validationResult.IsValid)
                {
                    foreach (var error in validationResult.Errors)
                    {
                        ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                    }
                    return BadRequest(ModelState);
                }

                await _authorService.UpdateAuthorAsync(id, updateAuthorDto);
                _logger.LogInformation("Author updated successfully: {AuthorId}", id);

                return NoContent();
            }
            catch (AuthorNotFoundException ex)
            {
                _logger.LogWarning("Author not found for update: {AuthorId}", id);
                return NotFound(new { message = ex.Message, authorId = ex.AuthorId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating author with ID: {AuthorId}", id);
                throw;
            }
        }

        /// <summary>
        /// Delete an author
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                _logger.LogInformation("Deleting author with ID: {AuthorId}", id);
                await _authorService.DeleteAuthorAsync(id);
                _logger.LogInformation("Author deleted successfully: {AuthorId}", id);

                return NoContent();
            }
            catch (AuthorNotFoundException ex)
            {
                _logger.LogWarning("Author not found for deletion: {AuthorId}", id);
                return NotFound(new { message = ex.Message, authorId = ex.AuthorId });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Cannot delete author: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting author with ID: {AuthorId}", id);
                throw;
            }
        }

        /// <summary>
        /// Check if author exists
        /// </summary>
        [HttpGet("{id}/exists")]
        public async Task<ActionResult<bool>> Exists(int id)
        {
            try
            {
                _logger.LogInformation("Checking if author exists: {AuthorId}", id);
                var exists = await _authorService.AuthorExistsAsync(id);
                return Ok(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if author exists: {AuthorId}", id);
                throw;
            }
        }
    }
} 