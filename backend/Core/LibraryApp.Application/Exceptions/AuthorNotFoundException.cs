namespace LibraryApp.Application.Exceptions
{
    public class AuthorNotFoundException : Exception
    {
        public int AuthorId { get; }

        public AuthorNotFoundException(int authorId) 
            : base($"Author with ID {authorId} was not found.")
        {
            AuthorId = authorId;
        }

        public AuthorNotFoundException(int authorId, string message) 
            : base(message)
        {
            AuthorId = authorId;
        }

        public AuthorNotFoundException(int authorId, string message, Exception innerException) 
            : base(message, innerException)
        {
            AuthorId = authorId;
        }
    }
} 