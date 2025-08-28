using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LibraryApp.Application.Interfaces;

namespace LibraryApp.Infrastructure.Auth
{
    public class EncryptedJwtHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IJwtService _jwtService;

        public EncryptedJwtHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            Microsoft.Extensions.Logging.ILoggerFactory logger,
            UrlEncoder encoder,
#pragma warning disable CS0618 // Type or member is obsolete
            ISystemClock clock,
#pragma warning restore CS0618 // Type or member is obsolete
            IJwtService jwtService)
#pragma warning disable CS0618 // Type or member is obsolete
            : base(options, logger, encoder, clock)
#pragma warning restore CS0618 // Type or member is obsolete
        {
            _jwtService = jwtService;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            try
            {
                Console.WriteLine($"=== Authentication Request Started ===");
                Console.WriteLine($"Request timestamp: {DateTime.UtcNow}");
                Console.WriteLine($"Request path: {Request.Path}");
                Console.WriteLine($"Request method: {Request.Method}");
                
                string token = null;

                // Only accept token from Authorization header (Bearer)
                if (Request.Headers.ContainsKey("Authorization"))
                {
                    var authorizationHeader = Request.Headers["Authorization"].ToString();
                    Console.WriteLine($"Authorization header: {authorizationHeader.Substring(0, Math.Min(50, authorizationHeader.Length))}...");
                    
                    // Bearer prefix'i yoksa otomatik ekle
                    token = authorizationHeader.Trim();
                    if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    {
                        token = token.Substring("Bearer ".Length).Trim();
                    }
                }
                else
                {
                    Console.WriteLine("No Authorization header found.");
                    return AuthenticateResult.Fail("No token found in Authorization header.");
                }
                Console.WriteLine($"Token length: {token.Length}");

                if (string.IsNullOrEmpty(token))
                {
                    Console.WriteLine("Token is empty.");
                    return AuthenticateResult.Fail("Token is empty.");
                }

                // Token'ı çöz ve userId'yi al
                Console.WriteLine("Extracting userId from token...");
                var userId = await ExtractUserIdFromToken(token);
                Console.WriteLine($"Extracted userId: {userId}");
                
                if (userId.HasValue)
                {
                    var claims = new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()),
                        new Claim("userId", userId.Value.ToString())
                    };

                    var identity = new ClaimsIdentity(claims, Scheme.Name);
                    var principal = new ClaimsPrincipal(identity);
                    var ticket = new AuthenticationTicket(principal, Scheme.Name);

                    Console.WriteLine($"Authentication successful for userId: {userId.Value}");
                    return AuthenticateResult.Success(ticket);
                }

                Console.WriteLine("Invalid token - userId extraction failed.");
                return AuthenticateResult.Fail("Invalid token.");
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                Console.WriteLine($"Authentication error: {ex.Message}");
                Console.WriteLine($"Exception type: {ex.GetType().Name}");
                var logPath = Path.Combine(Directory.GetCurrentDirectory(), "logs", "auth_errors.log");
                var logDirectory = Path.GetDirectoryName(logPath);
                if (!string.IsNullOrEmpty(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }
                File.AppendAllText(logPath, $"{DateTime.Now}: Authentication error: {ex.Message}\n{ex.StackTrace}\n");
                
                return AuthenticateResult.Fail($"Token validation failed: {ex.Message}");
            }
        }

        private async Task<int?> ExtractUserIdFromToken(string token)
        {
            try
            {
                // Yeni JWT servisi ile token doğrulama
                var principal = await _jwtService.ValidateAccessToken(token);
                
                // Claims'den userId'yi çıkar
                var userIdClaim = principal.FindFirst("sub") ?? principal.FindFirst("userId");
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    // Debug için log ekle
                    Console.WriteLine($"Token parse edildi, userId: {userId}");
                    
                    // Log dosyasına da yaz
                    var logPath = Path.Combine(Directory.GetCurrentDirectory(), "logs", "debug.log");
                    var logDirectory = Path.GetDirectoryName(logPath);
                    if (!string.IsNullOrEmpty(logDirectory))
                    {
                        Directory.CreateDirectory(logDirectory);
                    }
                    File.AppendAllText(logPath, $"{DateTime.Now}: Token parse edildi, userId: {userId}\n");
                    
                    return userId;
                }
                
                return null;
            }
            catch (Exception ex)
            {
                // Debug için hata logla
                Console.WriteLine($"Token çözme hatası: {ex.Message}");
                
                // Log dosyasına da yaz
                var logPath = Path.Combine(Directory.GetCurrentDirectory(), "logs", "debug.log");
                var logDirectory = Path.GetDirectoryName(logPath);
                if (!string.IsNullOrEmpty(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }
                File.AppendAllText(logPath, $"{DateTime.Now}: Token çözme hatası: {ex.Message}\n{ex.StackTrace}\n");
                
                // Return null instead of re-throwing to prevent authentication failures
                return null;
            }
        }

        private int? ParseNormalJwtToken(string token)
        {
            try
            {
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var jsonToken = handler.ReadJwtToken(token);
                
                var userIdClaim = jsonToken.Claims.FirstOrDefault(x => x.Type == "sub" || x.Type == "userId");
                
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    return userId;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        private bool IsBase64String(string str)
        {
            try
            {
                Convert.FromBase64String(str);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
} 