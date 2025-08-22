using LibraryApp.Application.Exceptions;
using System.Net;
using System.Text.Json;
using FluentValidation;
using AppValidationException = LibraryApp.Application.Exceptions.ValidationException;
using System.IO;
using LibraryApp.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace LibraryApp.Infrastructure.Middleware
{
    /// <summary>
    /// Global exception handling middleware for the application
    /// </summary>
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly ILoggingService _loggingService;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, ILoggingService loggingService)
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
            var userAgent = context.Request.Headers["User-Agent"].ToString();
            var remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userId = GetUserIdFromToken(context);
            
            try
            {
                await _next(context);
                
                var endTime = DateTime.UtcNow;
                var duration = (endTime - startTime).TotalMilliseconds;
                var statusCode = context.Response.StatusCode;
                
                // Log successful operations for specific endpoints
                LogSuccessfulOperation(context, duration, userId);
            }
            catch (Exception ex)
            {
                var endTime = DateTime.UtcNow;
                var duration = (endTime - startTime).TotalMilliseconds;
                
                LogExceptionToService(ex, context, duration, userId);
                await HandleExceptionAsync(context, ex);
            }
        }

        private void LogSuccessfulOperation(HttpContext context, double duration, int? userId)
        {
            var endpoint = context.Request.Path.Value?.ToLower();
            var method = context.Request.Method;
            var statusCode = context.Response.StatusCode;

            // Only log successful operations (2xx status codes)
            if (statusCode >= 200 && statusCode < 300)
            {
                // Log specific operations for Book and Author modules
                if (endpoint?.Contains("/books") == true || endpoint?.Contains("/authors") == true || 
                    endpoint?.Contains("/publishers") == true || endpoint?.Contains("/users") == true)
                {
                    var operation = GetOperationDescription(method, endpoint);
                    _loggingService.LogSystemAction("SUCCESS_OPERATION", $"Operation completed successfully", new { 
                        Method = method,
                        Endpoint = endpoint,
                        StatusCode = statusCode,
                        Operation = operation,
                        Duration = duration,
                        UserId = userId 
                    });
                }
            }
        }

        private string GetOperationDescription(string method, string? endpoint)
        {
            if (endpoint == null) return "Unknown";

            if (endpoint.Contains("/books"))
            {
                return method switch
                {
                    "GET" => "Get Books",
                    "POST" => endpoint.Contains("/borrow") ? "Borrow Book" : "Create Book",
                    "PUT" => "Update Book",
                    "DELETE" => "Delete Book",
                    _ => "Book Operation"
                };
            }
            else if (endpoint.Contains("/authors"))
            {
                return method switch
                {
                    "GET" => "Get Authors",
                    "POST" => "Create Author",
                    "PUT" => "Update Author",
                    "DELETE" => "Delete Author",
                    _ => "Author Operation"
                };
            }
            else if (endpoint.Contains("/publishers"))
            {
                return method switch
                {
                    "GET" => "Get Publishers",
                    "POST" => "Create Publisher",
                    "PUT" => "Update Publisher",
                    "DELETE" => "Delete Publisher",
                    _ => "Publisher Operation"
                };
            }
            else if (endpoint.Contains("/users"))
            {
                return method switch
                {
                    "GET" => "Get Users",
                    "POST" => endpoint.Contains("/login") ? "User Login" : "User Register",
                    "PUT" => "Update User",
                    "DELETE" => "Delete User",
                    _ => "User Operation"
                };
            }

            return "Unknown Operation";
        }

        private void LogExceptionToService(Exception ex, HttpContext context, double duration, int? userId)
        {
            var endpoint = context.Request.Path.Value?.ToLower();
            var method = context.Request.Method;
            var userAgent = context.Request.Headers["User-Agent"].ToString();
            var remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var statusCode = GetStatusCodeForException(ex);
            
            // Log to error file using ILoggingService
            _loggingService.LogError(ex, "HTTP_REQUEST_ERROR", new { 
                Method = method,
                Endpoint = endpoint,
                StatusCode = statusCode,
                Duration = duration,
                UserId = userId,
                RemoteIP = remoteIp,
                UserAgent = userAgent
            });
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

        private int GetStatusCodeForException(Exception ex)
        {
            return ex switch
            {
                BookNotFoundException or AuthorNotFoundException or PublisherNotFoundException or UserNotFoundException => 404,
                BookNotAvailableException or DuplicateEmailException => 409,
                AppValidationException or FluentValidation.ValidationException => 400,
                ArgumentNullException or ArgumentException => 400,
                UnauthorizedAccessException => 401,
                KeyNotFoundException => 404,
                InvalidOperationException => 400,
                NotSupportedException => 501,
                _ => 500
            };
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var response = context.Response;
            response.ContentType = "application/json";

            var errorResponse = new ErrorResponse();

            switch (exception)
            {
                // Book related exceptions
                case BookNotFoundException bookNotFound:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    errorResponse = new ErrorResponse
                    {
                        Message = bookNotFound.Message,
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Details = new { BookId = bookNotFound.BookId }
                    };
                    break;

                case BookNotAvailableException bookNotAvailable:
                    response.StatusCode = (int)HttpStatusCode.Conflict;
                    errorResponse = new ErrorResponse
                    {
                        Message = bookNotAvailable.Message,
                        StatusCode = (int)HttpStatusCode.Conflict,
                        Details = new { BookId = bookNotAvailable.BookId, BookTitle = bookNotAvailable.BookTitle }
                    };
                    break;

                // Author related exceptions
                case AuthorNotFoundException authorNotFound:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    errorResponse = new ErrorResponse
                    {
                        Message = authorNotFound.Message,
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Details = new { AuthorId = authorNotFound.AuthorId }
                    };
                    break;

                // Publisher related exceptions
                case PublisherNotFoundException publisherNotFound:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    errorResponse = new ErrorResponse
                    {
                        Message = publisherNotFound.Message,
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Details = new { PublisherId = publisherNotFound.PublisherId }
                    };
                    break;

                // User related exceptions
                case UserNotFoundException userNotFound:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    errorResponse = new ErrorResponse
                    {
                        Message = userNotFound.Message,
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Details = new { UserId = userNotFound.UserId, Email = userNotFound.Email }
                    };
                    break;

                case DuplicateEmailException duplicateEmail:
                    response.StatusCode = (int)HttpStatusCode.Conflict;
                    errorResponse = new ErrorResponse
                    {
                        Message = duplicateEmail.Message,
                        StatusCode = (int)HttpStatusCode.Conflict,
                        Details = new { Email = duplicateEmail.Email }
                    };
                    break;

                // Validation exceptions
                case AppValidationException validationEx:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse = new ErrorResponse
                    {
                        Message = validationEx.Message,
                        StatusCode = (int)HttpStatusCode.BadRequest,
                        Details = validationEx.Errors
                    };
                    break;

                case FluentValidation.ValidationException fluentValidationEx:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    var errors = fluentValidationEx.Errors.Select(e => new { Field = e.PropertyName, Message = e.ErrorMessage }).ToList();
                    errorResponse = new ErrorResponse
                    {
                        Message = "Validation failed",
                        StatusCode = (int)HttpStatusCode.BadRequest,
                        Details = errors
                    };
                    break;

                // General exceptions
                case ArgumentNullException argNullEx:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse = new ErrorResponse
                    {
                        Message = argNullEx.Message,
                        StatusCode = (int)HttpStatusCode.BadRequest
                    };
                    break;

                case ArgumentException argEx:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse = new ErrorResponse
                    {
                        Message = argEx.Message,
                        StatusCode = (int)HttpStatusCode.BadRequest
                    };
                    break;

                case UnauthorizedAccessException unauthorizedEx:
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    errorResponse = new ErrorResponse
                    {
                        Message = unauthorizedEx.Message,
                        StatusCode = (int)HttpStatusCode.Unauthorized
                    };
                    break;

                case KeyNotFoundException keyNotFoundEx:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    errorResponse = new ErrorResponse
                    {
                        Message = keyNotFoundEx.Message,
                        StatusCode = (int)HttpStatusCode.NotFound
                    };
                    break;

                case InvalidOperationException invalidOpEx:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse = new ErrorResponse
                    {
                        Message = invalidOpEx.Message,
                        StatusCode = (int)HttpStatusCode.BadRequest
                    };
                    break;

                case NotSupportedException notSupportedEx:
                    response.StatusCode = (int)HttpStatusCode.NotImplemented;
                    errorResponse = new ErrorResponse
                    {
                        Message = notSupportedEx.Message,
                        StatusCode = (int)HttpStatusCode.NotImplemented
                    };
                    break;

                default:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    errorResponse = new ErrorResponse
                    {
                        Message = "An internal server error occurred",
                        StatusCode = (int)HttpStatusCode.InternalServerError
                    };
                    break;
            }

            var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await response.WriteAsync(jsonResponse);
        }
    }

    public class ErrorResponse
    {
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public object? Details { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string TraceId { get; set; } = Guid.NewGuid().ToString();
    }
} 