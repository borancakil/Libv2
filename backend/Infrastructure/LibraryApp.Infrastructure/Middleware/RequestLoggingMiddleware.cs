using LibraryApp.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace LibraryApp.Infrastructure.Middleware
{
    /// <summary>
    /// Middleware for logging HTTP requests and performance metrics
    /// </summary>
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;
        private readonly ILoggingService _loggingService;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger, ILoggingService loggingService)
        {
            _next = next;
            _logger = logger;
            _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var startTime = DateTime.UtcNow;
            var requestPath = context.Request.Path.Value;
            var requestMethod = context.Request.Method;
            var userId = GetUserIdFromToken(context);
            var requestId = Guid.NewGuid().ToString();

            // Add request ID to context for tracking
            context.Items["RequestId"] = requestId;

            // Only log CRUD operations and important requests
            var shouldLog = ShouldLogRequest(requestMethod, requestPath);
            
            if (shouldLog)
            {
                _loggingService.LogSystemAction("REQUEST_START", $"Request started", new { 
                    RequestId = requestId,
                    Method = requestMethod,
                    Path = requestPath,
                    UserId = userId,
                    Timestamp = startTime
                });
            }

            try
            {
                // Process the request
                await _next(context);

                var endTime = DateTime.UtcNow;
                var duration = (endTime - startTime).TotalMilliseconds;
                var statusCode = context.Response.StatusCode;

                // Only log completion for CRUD operations and important requests
                if (shouldLog)
                {
                    // Log request completion
                    _loggingService.LogSystemAction("REQUEST_COMPLETE", $"Request completed", new { 
                        RequestId = requestId,
                        Method = requestMethod,
                        Path = requestPath,
                        StatusCode = statusCode,
                        Duration = duration,
                        UserId = userId,
                        Timestamp = endTime
                    });

                    // Log performance metrics for slow requests (only for CRUD operations)
                    if (duration > 3000) // Increased threshold to 3 seconds for CRUD operations
                    {
                        _loggingService.LogSystemAction("PERFORMANCE_WARNING", $"Slow CRUD request detected", new { 
                            RequestId = requestId,
                            Method = requestMethod,
                            Path = requestPath,
                            Duration = duration,
                            UserId = userId
                        });
                    }
                }

                // Log memory usage for monitoring (only for high usage and CRUD operations)
                var memoryUsage = GC.GetTotalMemory(false);
                if (memoryUsage > 300 * 1024 * 1024 && shouldLog) // Increased threshold to 300MB
                {
                    _loggingService.LogSystemAction("MEMORY_WARNING", $"High memory usage detected", new { 
                        RequestId = requestId,
                        MemoryUsageMB = memoryUsage / (1024 * 1024),
                        Method = requestMethod,
                        Path = requestPath,
                        UserId = userId
                    });
                }
            }
            catch (Exception ex)
            {
                var endTime = DateTime.UtcNow;
                var duration = (endTime - startTime).TotalMilliseconds;

                // Always log request failures
                _loggingService.LogSystemAction("REQUEST_FAILED", $"Request failed", new { 
                    RequestId = requestId,
                    Method = requestMethod,
                    Path = requestPath,
                    Duration = duration,
                    UserId = userId,
                    ExceptionType = ex.GetType().Name,
                    ExceptionMessage = ex.Message,
                    Timestamp = endTime
                });

                // Re-throw the exception for GlobalExceptionMiddleware to handle
                throw;
            }
        }

        private int? GetUserIdFromToken(HttpContext context)
        {
            try
            {
                var userIdClaim = context.User.FindFirst("userId");
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    return userId;
                }
            }
            catch
            {
                // Ignore token parsing errors
            }
            return null;
        }

        private bool IsStaticResource(string? path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            var staticExtensions = new[] { ".css", ".js", ".png", ".jpg", ".jpeg", ".gif", ".ico", ".svg", ".woff", ".woff2", ".ttf", ".eot" };
            return staticExtensions.Any(ext => path.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
        }

        private bool IsHealthCheck(string? path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            return path.Contains("/health", StringComparison.OrdinalIgnoreCase) ||
                   path.Contains("/favicon", StringComparison.OrdinalIgnoreCase);
        }

        private bool ShouldLogRequest(string method, string? path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            // Skip static resources
            if (IsStaticResource(path))
                return false;

            // Skip health checks
            if (IsHealthCheck(path))
                return false;

            // Skip OPTIONS requests (CORS preflight)
            if (method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
                return false;

            // Skip HEAD requests
            if (method.Equals("HEAD", StringComparison.OrdinalIgnoreCase))
                return false;

            // Skip GET requests (read operations)
            if (method.Equals("GET", StringComparison.OrdinalIgnoreCase))
                return false;

            // Only log write operations (POST, PUT, DELETE, PATCH)
            var writeMethods = new[] { "POST", "PUT", "DELETE", "PATCH" };
            if (!writeMethods.Contains(method.ToUpper()))
                return false;

            // Log all write operations on main API endpoints
            if (path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
                return true;

            // Skip other requests
            return false;
        }
    }
}
