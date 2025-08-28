using System;

namespace LibraryApp.Application.Exceptions
{
    /// <summary>
    /// Exception thrown when trying to register with an email that already exists
    /// </summary>
    public class DuplicateEmailException : Exception
    {
        public string Email { get; }

        public DuplicateEmailException(string email) 
            : base($"An account with email '{email}' already exists.")
        {
            Email = email;
        }

        public DuplicateEmailException(string email, string message) 
            : base(message)
        {
            Email = email;
        }

        public DuplicateEmailException(string email, string message, Exception innerException) 
            : base(message, innerException)
        {
            Email = email;
        }
    }
} 