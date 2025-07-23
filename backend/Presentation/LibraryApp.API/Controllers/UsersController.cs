using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LibraryApp.Application.DTOs.User;
using LibraryApp.Application.Exceptions;
using LibraryApp.Application.Interfaces;

namespace LibraryApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// User login endpoint
        /// </summary>
        /// <param name="dto">Login credentials</param>
        /// <returns>JWT token</returns>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginUserDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var token = await _userService.LoginAsync(dto.Email, dto.Password);
                return Ok(new { token, message = "Login successful" });
            }
            catch (UserNotFoundException ex)
            {
                _logger.LogWarning("Login attempt failed - user not found: {Email}", ex.Email);
                return Unauthorized(new { message = "Invalid email or password" });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Login attempt failed - invalid password for email: {Email}", dto.Email);
                return Unauthorized(new { message = ex.Message });
            }
        }

        /// <summary>
        /// User registration endpoint
        /// </summary>
        /// <param name="dto">Registration data</param>
        /// <returns>Success message with user ID</returns>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] AddUserDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var userId = await _userService.AddUserAsync(dto);
                var createdUser = await _userService.GetByIdAsync(userId);
                
                return CreatedAtAction(
                    nameof(GetById), 
                    new { id = userId }, 
                    new { message = "User registered successfully", user = createdUser }
                );
            }
            catch (DuplicateEmailException ex)
            {
                _logger.LogWarning("Registration failed - duplicate email: {Email}", ex.Email);
                return Conflict(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get all users (Admin only)
        /// </summary>
        /// <param name="includeLoans">Whether to include loan information</param>
        /// <returns>List of users</returns>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers([FromQuery] bool includeLoans = false)
        {
            try
            {
                var users = await _userService.GetAllUsersAsync(includeLoans);
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users");
                return StatusCode(500, new { message = "An error occurred while retrieving users" });
            }
        }

        /// <summary>
        /// Get user by ID (Admin only)
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>User details</returns>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { message = "Invalid user ID" });
            }

            try
            {
                var user = await _userService.GetByIdAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = $"User with ID {id} not found" });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user with ID {UserId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving the user" });
            }
        }

        /// <summary>
        /// Update user profile (Admin only)
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="dto">Updated user data</param>
        /// <returns>No content on success</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
        {
            if (id <= 0)
            {
                return BadRequest(new { message = "Invalid user ID" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _userService.UpdateUserAsync(id, dto);
                return NoContent();
            }
            catch (UserNotFoundException ex)
            {
                _logger.LogWarning("User not found for update: {UserId}", ex.UserId);
                return NotFound(new { message = ex.Message });
            }
            catch (DuplicateEmailException ex)
            {
                _logger.LogWarning("Update failed - duplicate email: {Email}", ex.Email);
                return Conflict(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Promote user to admin role (Admin only)
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>No content on success</returns>
        [HttpPut("{id}/promote")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PromoteToAdmin(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { message = "Invalid user ID" });
            }

            try
            {
                await _userService.PromoteUserToAdminAsync(id);
                return NoContent();
            }
            catch (UserNotFoundException ex)
            {
                _logger.LogWarning("User not found for promotion: {UserId}", ex.UserId);
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Demote user to regular role (Admin only)
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>No content on success</returns>
        [HttpPut("{id}/demote")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DemoteToUser(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { message = "Invalid user ID" });
            }

            try
            {
                await _userService.DemoteUserToRegularAsync(id);
                return NoContent();
            }
            catch (UserNotFoundException ex)
            {
                _logger.LogWarning("User not found for demotion: {UserId}", ex.UserId);
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update user password (Authenticated users can update their own password)
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="dto">Password update data</param>
        /// <returns>No content on success</returns>
        [HttpPut("{id}/password")]
        [Authorize]
        public async Task<IActionResult> UpdatePassword(int id, [FromBody] UpdatePasswordUserDto dto)
        {
            if (id <= 0)
            {
                return BadRequest(new { message = "Invalid user ID" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _userService.UpdatePasswordAsync(id, dto);
                return NoContent();
            }
            catch (UserNotFoundException ex)
            {
                _logger.LogWarning("User not found for password update: {UserId}", ex.UserId);
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Password update failed - incorrect current password for user: {UserId}", id);
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete user (Admin only)
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>No content on success</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { message = "Invalid user ID" });
            }

            try
            {
                await _userService.DeleteUserAsync(id);
                return NoContent();
            }
            catch (UserNotFoundException ex)
            {
                _logger.LogWarning("User not found for deletion: {UserId}", ex.UserId);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Cannot delete user: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Check if user exists (Admin only)
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>Existence status</returns>
        [HttpHead("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UserExists(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            try
            {
                var exists = await _userService.UserExistsAsync(id);
                return exists ? Ok() : NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking user existence with ID {UserId}", id);
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Check if email exists (Public endpoint for registration validation)
        /// </summary>
        /// <param name="email">Email address</param>
        /// <returns>Existence status</returns>
        [HttpGet("email-exists")]
        [AllowAnonymous]
        public async Task<IActionResult> EmailExists([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest(new { message = "Email is required" });
            }

            try
            {
                var exists = await _userService.EmailExistsAsync(email);
                return Ok(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking email existence: {Email}", email);
                return StatusCode(500, new { message = "An error occurred while checking email" });
            }
        }
    }
}