namespace LibraryApp.Application.Exceptions
{
    /// <summary>
    /// Exception thrown when trying to borrow a book that is not available
    /// </summary>
    public class BookNotAvailableException : Exception
    {
        public int BookId { get; }
        public string BookTitle { get; }

        public BookNotAvailableException(int bookId, string bookTitle) 
            : base($"Book '{bookTitle}' (ID: {bookId}) is not available for borrowing.")
        {
            BookId = bookId;
            BookTitle = bookTitle;
        }

        public BookNotAvailableException(int bookId, string bookTitle, string message) 
            : base(message)
        {
            BookId = bookId;
            BookTitle = bookTitle;
        }

        public BookNotAvailableException(int bookId, string bookTitle, string message, Exception innerException) 
            : base(message, innerException)
        {
            BookId = bookId;
            BookTitle = bookTitle;
        }
    }
} 