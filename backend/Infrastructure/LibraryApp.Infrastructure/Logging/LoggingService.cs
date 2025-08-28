using LibraryApp.Domain.Interfaces;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace LibraryApp.Infrastructure.Logging
{
    public class LoggingService : ILoggingService
    {
        private readonly ILogger<LoggingService> _logger;

        public LoggingService(ILogger<LoggingService> logger)
        {
            _logger = logger;
        }

        public void LogUserAction(int userId, string action, string details, object? data = null)
        {
            var logEntry = new
            {
                Timestamp = DateTime.UtcNow,
                Type = "USER_ACTION",
                UserId = userId,
                Action = action,
                Details = details,
                Data = data
            };

            _logger.LogInformation(
                "User Action: User {UserId} performed {Action} - {Details} {@Data}", 
                userId, action, details, data);
        }

        public void LogBookAction(int userId, string action, int bookId, string bookTitle, object? data = null)
        {
            var logEntry = new
            {
                Timestamp = DateTime.UtcNow,
                Type = "BOOK_ACTION",
                UserId = userId,
                Action = action,
                BookId = bookId,
                BookTitle = bookTitle,
                Data = data
            };

            _logger.LogInformation(
                "Book Action: User {UserId} {Action} book '{BookTitle}' (ID: {BookId}) {@Data}", 
                userId, action, bookTitle, bookId, data);
        }

        public void LogDataOperation(string operation, string table, int recordId, object? data = null)
        {
            var logEntry = new
            {
                Timestamp = DateTime.UtcNow,
                Type = "DATA_OPERATION",
                Operation = operation,
                Table = table,
                RecordId = recordId,
                Data = data
            };

            _logger.LogInformation(
                "Data Operation: {Operation} on {Table} (ID: {RecordId}) {@Data}", 
                operation, table, recordId, data);
        }

        public void LogHttpRequest(string method, string path, int? userId = null, object? data = null)
        {
            var logEntry = new
            {
                Timestamp = DateTime.UtcNow,
                Type = "HTTP_REQUEST",
                Method = method,
                Path = path,
                UserId = userId,
                Data = data
            };

            _logger.LogInformation(
                "HTTP Request: {Method} {Path} by User {UserId} {@Data}", 
                method, path, userId ?? 0, data);
        }

        public void LogSystemAction(string action, string details, object? data = null)
        {
            var logEntry = new
            {
                Timestamp = DateTime.UtcNow,
                Type = "SYSTEM_ACTION",
                Action = action,
                Details = details,
                Data = data
            };

            _logger.LogInformation(
                "System Action: {Action} - {Details} {@Data}", 
                action, details, data);
        }

        public void LogCrudOperation(string operation, string entity, int? entityId = null, int? userId = null, object? data = null)
        {
            var logEntry = new
            {
                Timestamp = DateTime.UtcNow,
                Type = "CRUD_OPERATION",
                Operation = operation,
                Entity = entity,
                EntityId = entityId,
                UserId = userId,
                Data = data
            };

            _logger.LogInformation(
                "CRUD Operation: {Operation} on {Entity} (ID: {EntityId}) by User {UserId} {@Data}", 
                operation, entity, entityId ?? 0, userId ?? 0, data);
        }


        public void LogError(Exception ex, string context, object? data = null)
        {
            var logEntry = new
            {
                Timestamp = DateTime.UtcNow,
                Type = "ERROR",
                Context = context,
                ExceptionType = ex.GetType().Name,
                Message = ex.Message,
                StackTrace = ex.StackTrace,
                InnerException = ex.InnerException?.Message,
                Data = data
            };

            _logger.LogError(
                ex,
                "Error in {Context}: {Message} {@Data}",
                context, ex.Message, data);
        }
    }
}
