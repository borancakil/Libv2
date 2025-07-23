using Microsoft.AspNetCore.Mvc;
using LibraryApp.Application.DTOs.Publisher;
using LibraryApp.Application.Services;
using LibraryApp.Application.Exceptions;
using FluentValidation;

namespace LibraryApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PublishersController : ControllerBase
    {
        private readonly PublisherService _publisherService;
        private readonly IValidator<CreatePublisherDto> _createValidator;
        private readonly IValidator<UpdatePublisherDto> _updateValidator;
        private readonly ILogger<PublishersController> _logger;

        public PublishersController(
            PublisherService publisherService,
            IValidator<CreatePublisherDto> createValidator,
            IValidator<UpdatePublisherDto> updateValidator,
            ILogger<PublishersController> logger)
        {
            _publisherService = publisherService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _logger = logger;
        }

        /// <summary>
        /// Get all publishers
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PublisherDto>>> GetAll()
        {
            try
            {
                _logger.LogInformation("Getting all publishers");
                var publishers = await _publisherService.GetAllPublishersAsync();
                return Ok(publishers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all publishers");
                throw;
            }
        }

        /// <summary>
        /// Get publisher by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<PublisherDto>> GetById(int id)
        {
            try
            {
                _logger.LogInformation("Getting publisher with ID: {PublisherId}", id);
                var publisher = await _publisherService.GetPublisherByIdAsync(id);
                return Ok(publisher);
            }
            catch (PublisherNotFoundException ex)
            {
                _logger.LogWarning("Publisher not found: {PublisherId}", id);
                return NotFound(new { message = ex.Message, publisherId = ex.PublisherId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting publisher with ID: {PublisherId}", id);
                throw;
            }
        }

        /// <summary>
        /// Create a new publisher
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<PublisherDto>> Create([FromBody] CreatePublisherDto createPublisherDto)
        {
            try
            {
                _logger.LogInformation("Creating new publisher: {PublisherName}", createPublisherDto.Name);

                // Validate
                var validationResult = await _createValidator.ValidateAsync(createPublisherDto);
                if (!validationResult.IsValid)
                {
                    foreach (var error in validationResult.Errors)
                    {
                        ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                    }
                    return BadRequest(ModelState);
                }

                var createdPublisher = await _publisherService.CreatePublisherAsync(createPublisherDto);
                _logger.LogInformation("Publisher created successfully with ID: {PublisherId}", createdPublisher.PublisherId);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = createdPublisher.PublisherId },
                    createdPublisher);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating publisher: {PublisherName}", createPublisherDto.Name);
                throw;
            }
        }

        /// <summary>
        /// Update an existing publisher
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePublisherDto updatePublisherDto)
        {
            try
            {
                _logger.LogInformation("Updating publisher with ID: {PublisherId}", id);

                // Validate
                var validationResult = await _updateValidator.ValidateAsync(updatePublisherDto);
                if (!validationResult.IsValid)
                {
                    foreach (var error in validationResult.Errors)
                    {
                        ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                    }
                    return BadRequest(ModelState);
                }

                await _publisherService.UpdatePublisherAsync(id, updatePublisherDto);
                _logger.LogInformation("Publisher updated successfully: {PublisherId}", id);

                return NoContent();
            }
            catch (PublisherNotFoundException ex)
            {
                _logger.LogWarning("Publisher not found for update: {PublisherId}", id);
                return NotFound(new { message = ex.Message, publisherId = ex.PublisherId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating publisher with ID: {PublisherId}", id);
                throw;
            }
        }

        /// <summary>
        /// Delete a publisher
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                _logger.LogInformation("Deleting publisher with ID: {PublisherId}", id);
                await _publisherService.DeletePublisherAsync(id);
                _logger.LogInformation("Publisher deleted successfully: {PublisherId}", id);

                return NoContent();
            }
            catch (PublisherNotFoundException ex)
            {
                _logger.LogWarning("Publisher not found for deletion: {PublisherId}", id);
                return NotFound(new { message = ex.Message, publisherId = ex.PublisherId });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Cannot delete publisher: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting publisher with ID: {PublisherId}", id);
                throw;
            }
        }

        /// <summary>
        /// Check if publisher exists
        /// </summary>
        [HttpGet("{id}/exists")]
        public async Task<ActionResult<bool>> Exists(int id)
        {
            try
            {
                _logger.LogInformation("Checking if publisher exists: {PublisherId}", id);
                var exists = await _publisherService.PublisherExistsAsync(id);
                return Ok(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if publisher exists: {PublisherId}", id);
                throw;
            }
        }
    }
} 