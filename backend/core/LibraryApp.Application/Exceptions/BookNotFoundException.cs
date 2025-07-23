namespace LibraryApp.Application.Exceptions
{
    /// <summary>
    /// Exception thrown when a requested book is not found
    /// </summary>
    public class BookNotFoundException : Exception
    {
        public int BookId { get; }

        public BookNotFoundException(int bookId) 
            : base($"Book with ID {bookId} was not found.")
        {
            BookId = bookId;
        }

        public BookNotFoundException(int bookId, string message) 
            : base(message)
        {
            BookId = bookId;
        }

        public BookNotFoundException(int bookId, string message, Exception innerException) 
            : base(message, innerException)
        {
            BookId = bookId;
        }
    }
} 