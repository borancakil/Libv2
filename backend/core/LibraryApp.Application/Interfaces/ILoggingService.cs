using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryApp.Domain.Interfaces
{
    public interface ILoggingService
    {
        void LogUserAction(int userId, string action, string details, object? data = null);
        void LogBookAction(int userId, string action, int bookId, string bookTitle, object? data = null);
        void LogDataOperation(string operation, string table, int recordId, object? data = null);
        void LogHttpRequest(string method, string path, int? userId = null, object? data = null);
        void LogSystemAction(string action, string details, object? data = null);
        void LogCrudOperation(string operation, string entity, int? entityId = null, int? userId = null, object? data = null);
        void LogError(Exception ex, string context, object? data = null);
    }
}
