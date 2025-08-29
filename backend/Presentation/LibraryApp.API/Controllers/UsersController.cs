using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LibraryApp.Application.DTOs.User;
using LibraryApp.Application.Interfaces;

namespace LibraryApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;
        private readonly IJwtService _jwtService;

        public UsersController(IUserService userService, ILogger<UsersController> logger, IJwtService jwtService)
        {
            _userService = userService;
            _logger = logger;
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginUserDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _userService.LoginAsync(dto.Email, dto.Password);

            // Create tokens
            var accessToken = _jwtService.CreateAccessToken(user.UserId, user.Email, user.Role.ToString());
            var refreshToken = _jwtService.CreateRefreshToken(user.UserId);

            _logger.LogInformation("Login attempt for user {Email}. Access token length: {AccessLength}, Refresh token length: {RefreshLength}", 
                user.Email, accessToken.Length, refreshToken.Length);

            // Return tokens in response body (client will store in localStorage)
            return Ok(new { message = "Login successful", accessToken, refreshToken, tokenType = "Bearer", expiresIn = (int)_jwtService.AccessLifetime.TotalSeconds });
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] AddUserDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var userId = await _userService.AddUserAsync(dto);
            var createdUser = await _userService.GetByIdAsync(userId);
            return CreatedAtAction(nameof(GetById), new { id = userId }, createdUser);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers([FromQuery] bool includeLoans = false)
        {
            var users = await _userService.GetAllUsersAsync(includeLoans);
            return Ok(users);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0) return BadRequest(new { message = "Invalid user ID" });
            var user = await _userService.GetByIdAsync(id);
            if (user == null) return NotFound(new { message = $"User with ID {id} not found" });
            return Ok(user);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
        {
            if (id <= 0) return BadRequest(new { message = "Invalid user ID" });
            if (!ModelState.IsValid) return BadRequest(ModelState);
            await _userService.UpdateUserAsync(id, dto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (id <= 0) return BadRequest(new { message = "Invalid user ID" });
            await _userService.DeleteUserAsync(id);
            return NoContent();
        }

        [HttpHead("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UserExists(int id)
        {
            if (id <= 0) return BadRequest();
            var exists = await _userService.UserExistsAsync(id);
            return exists ? Ok() : NotFound();
        }

        [HttpPut("{id}/promote")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PromoteToAdmin(int id)
        {
            if (id <= 0) return BadRequest(new { message = "Invalid user ID" });
            await _userService.PromoteUserToAdminAsync(id);
            return NoContent();
        }

        [HttpPut("{id}/demote")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DemoteToUser(int id)
        {
            if (id <= 0) return BadRequest(new { message = "Invalid user ID" });
            await _userService.DemoteUserToRegularAsync(id);
            return NoContent();
        }

        [HttpPut("{id}/password")]
        [Authorize]
        public async Task<IActionResult> UpdatePassword(int id, [FromBody] UpdatePasswordUserDto dto)
        {
            if (id <= 0) return BadRequest(new { message = "Invalid user ID" });
            if (!ModelState.IsValid) return BadRequest(ModelState);
            await _userService.UpdatePasswordAsync(id, dto);
            return NoContent();
        }

        [HttpGet("current-user")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUserInfo()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized(new { message = "User ID not found in token" });
            
            var user = await _userService.GetByIdAsync(userId.Value);
            if (user == null)
                return NotFound(new { message = "User not found" });
            
            return Ok(user);
        }

        [HttpGet("my-borrowed-books")]
        [Authorize]
        public async Task<IActionResult> GetMyBorrowedBooks()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized(new { message = "User ID not found in token" });
            var books = await _userService.GetBorrowedBooksAsync(userId.Value);
            return Ok(books);
        }

        [HttpGet("my-favorite-books")]
        [Authorize]
        public async Task<IActionResult> GetMyFavoriteBooks()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized(new { message = "User ID not found in token" });
            var books = await _userService.GetFavoriteBooksAsync(userId.Value);
            return Ok(books);
        }

        [HttpPost("my-favorites/{bookId}")]
        [Authorize]
        public async Task<IActionResult> AddToMyFavorites(int bookId)
        {
            if (bookId <= 0) return BadRequest(new { message = "Invalid book ID" });
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized(new { message = "User ID not found in token" });
            await _userService.AddToFavoritesAsync(userId.Value, bookId);
            return NoContent();
        }

                [HttpDelete("my-favorites/{bookId}")]
        [Authorize]
        public async Task<IActionResult> RemoveFromMyFavorites(int bookId)
        {
            if (bookId <= 0) return BadRequest(new { message = "Invalid book ID" });
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized(new { message = "User ID not found in token" });

            var result = await _userService.RemoveFromFavoritesAsync(userId.Value, bookId);

            if (!result)
            {
                return NotFound(new { message = $"Book with ID {bookId} is not in your favorites." });
            }

            return NoContent();
        }

        [HttpGet("{userId}/borrowed-books")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetBorrowedBooks(int userId)
        {
            if (userId <= 0) return BadRequest(new { message = "Invalid user ID" });
            var books = await _userService.GetBorrowedBooksAsync(userId);
            return Ok(books);
        }

        [HttpGet("{userId}/favorite-books")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetFavoriteBooks(int userId)
        {
            if (userId <= 0) return BadRequest(new { message = "Invalid user ID" });
            var books = await _userService.GetFavoriteBooksAsync(userId);
            return Ok(books);
        }

        [HttpPost("{userId}/favorites/{bookId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddToFavorites(int userId, int bookId)
        {
            if (userId <= 0) return BadRequest(new { message = "Invalid user ID" });
            if (bookId <= 0) return BadRequest(new { message = "Invalid book ID" });
            await _userService.AddToFavoritesAsync(userId, bookId);
            return NoContent();
        }

        [HttpDelete("{userId}/favorites/{bookId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveFromFavorites(int userId, int bookId)
        {
            if (userId <= 0) return BadRequest(new { message = "Invalid user ID" });
            if (bookId <= 0) return BadRequest(new { message = "Invalid book ID" });
            await _userService.RemoveFromFavoritesAsync(userId, bookId);
            return NoContent();
        }

        [HttpGet("email-exists")]
        [AllowAnonymous]
        public async Task<IActionResult> EmailExists([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return BadRequest(new { message = "Email is required" });
            var exists = await _userService.EmailExistsAsync(email);
            return Ok(new { exists });
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized(new { message = "User ID not found in token" });
            var user = await _userService.GetByIdAsync(userId.Value);
            if (user == null)
                return NotFound(new { message = "User not found" });
            return Ok(user);
        }

        [HttpGet("debug-auth")]
        [Authorize]
        public IActionResult DebugAuthentication()
        {
            var allClaims = User.Claims.Select(c => new { Type = c.Type, Value = c.Value }).ToList();
            var userId = GetCurrentUserId();
            return Ok(new
            {
                message = "Authentication successful",
                userId = userId,
                claims = allClaims,
                isAuthenticated = User.Identity?.IsAuthenticated ?? false,
                authenticationType = User.Identity?.AuthenticationType
            });
        }

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            // Client will clear localStorage tokens
            return Ok(new { message = "Logout successful" });
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken()
        {
            try
            {
                // Expect refresh token via Authorization: Bearer <refreshToken>
                var authorization = Request.Headers["Authorization"].ToString();
                if (string.IsNullOrWhiteSpace(authorization) || !authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    return Unauthorized(new { message = "Authorization header with Bearer refresh token required" });
                var refreshToken = authorization.Substring("Bearer ".Length).Trim();

                // Refresh token'ı doğrula ve yeni access token oluştur
                var principal = await _jwtService.ValidateRefreshToken(refreshToken);
                
                if (principal == null)
                {
                    return Unauthorized(new { message = "Invalid refresh token" });
                }

                // User ID'yi al
                var userIdClaim = principal.FindFirst("sub");
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(new { message = "Invalid user ID in refresh token" });
                }

                // User bilgilerini al
                var user = await _userService.GetByIdAsync(userId);
                if (user == null)
                {
                    return Unauthorized(new { message = "User not found" });
                }

                // Yeni access token oluştur
                var newAccessToken = _jwtService.CreateAccessToken(user.UserId, user.Email, user.Role.ToString());
                
                // Return new access token in response body
                return Ok(new { message = "Token refreshed successfully", accessToken = newAccessToken, tokenType = "Bearer", expiresIn = (int)_jwtService.AccessLifetime.TotalSeconds });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return Unauthorized(new { message = "Token refresh failed" });
            }
        }

        // Cookie management endpoint removed; tokens stored client-side

        [HttpGet("debug-loans")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DebugLoans()
        {
            var allLoans = await _userService.GetAllLoansAsync();
            var loanData = allLoans.Select(l => new
            {
                LoanId = l.LoanId,
                UserId = l.UserId,
                BookId = l.BookId,
                BookTitle = l.Book?.Title,
                UserEmail = l.User?.Email,
                LoanDate = l.LoanDate,
                DueDate = l.DueDate,
                ReturnDate = l.ReturnDate,
                IsActive = l.IsActive()
            }).ToList();

            return Ok(new
            {
                totalLoans = loanData.Count,
                activeLoans = loanData.Count(l => l.IsActive),
                loans = loanData
            });
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