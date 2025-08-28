namespace LibraryApp.Application.Exceptions
{
    /// <summary>
    /// Exception thrown when a requested user is not found
    /// </summary>
    public class UserNotFoundException : Exception
    {
        public int UserId { get; }
        public string? Email { get; }

        public UserNotFoundException(int userId) 
            : base($"User with ID {userId} was not found.")
        {
            UserId = userId;
        }

        public UserNotFoundException(string email) 
            : base($"User with email '{email}' was not found.")
        {
            Email = email;
        }

        public UserNotFoundException(int userId, string message) 
            : base(message)
        {
            UserId = userId;
        }

        public UserNotFoundException(string email, string message, bool isEmail) 
            : base(message)
        {
            Email = email;
        }

        public UserNotFoundException(int userId, string message, Exception innerException) 
            : base(message, innerException)
        {
            UserId = userId;
        }
    }
} 