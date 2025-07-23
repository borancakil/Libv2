namespace LibraryApp.Application.DTOs.Book
{
    /// <summary>
    /// DTO representing a book for read operations
    /// </summary>
    public class BookDto
    {
        public int BookId { get; set; }
        
        public string Title { get; set; } = string.Empty;
        
        public int PublicationYear { get; set; }
        
        public bool IsAvailable { get; set; }
        
        public int AuthorId { get; set; }
        
        public string? AuthorName { get; set; }
        
        public int PublisherId { get; set; }
        
        public string? PublisherName { get; set; }
        
        /// <summary>
        /// Number of times this book has been borrowed
        /// </summary>
        public int BorrowCount { get; set; }
        
        /// <summary>
        /// Current borrower information (if applicable)
        /// </summary>
        public string? CurrentBorrower { get; set; }
    }
}