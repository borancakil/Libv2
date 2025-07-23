using LibraryApp.Application.Exceptions;
using System.Net;
using System.Text.Json;
using FluentValidation;
using AppValidationException = LibraryApp.Application.Exceptions.ValidationException;

namespace LibraryApp.API.Middleware
{
    /// <summary>
    /// Global exception handling middleware for the application
    /// </summary>
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
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
                        Details = new { 
                            UserId = userNotFound.UserId,
                            Email = userNotFound.Email
                        }
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
                        Message = "One or more validation errors occurred",
                        StatusCode = (int)HttpStatusCode.BadRequest,
                        Details = validationEx.Errors
                    };
                    break;

                case FluentValidation.ValidationException fluentValidationEx:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    var errors = fluentValidationEx.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
                    
                    errorResponse = new ErrorResponse
                    {
                        Message = "Validation failed",
                        StatusCode = (int)HttpStatusCode.BadRequest,
                        Details = errors
                    };
                    break;

                // Standard .NET exceptions
                case ArgumentNullException argNullEx:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse = new ErrorResponse
                    {
                        Message = "Invalid request data",
                        StatusCode = (int)HttpStatusCode.BadRequest,
                        Details = new { Parameter = argNullEx.ParamName }
                    };
                    break;

                case ArgumentException argEx:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse = new ErrorResponse
                    {
                        Message = argEx.Message,
                        StatusCode = (int)HttpStatusCode.BadRequest,
                        Details = new { Parameter = argEx.ParamName }
                    };
                    break;

                case UnauthorizedAccessException:
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    errorResponse = new ErrorResponse
                    {
                        Message = "Unauthorized access",
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

    /// <summary>
    /// Standard error response model
    /// </summary>
    public class ErrorResponse
    {
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public object? Details { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string TraceId { get; set; } = Guid.NewGuid().ToString();
    }
} 