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

        /// <summary>
        /// Indicates if the book has a cover image
        /// </summary>
        public bool HasCoverImage { get; set; }

        /// <summary>
        /// Image content type (e.g., image/jpeg, image/png)
        /// </summary>
        public string? ImageContentType { get; set; }

        /// <summary>
        /// Original file name of the cover image
        /// </summary>
        public string? ImageFileName { get; set; }

        /// <summary>
        /// Book rating (0-5)
        /// </summary>
        public decimal Rating { get; set; }

        /// <summary>
        /// Book category ID
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// Book category name
        /// </summary>
        public string CategoryName { get; set; } = string.Empty;

        /// <summary>
        /// Book category description
        /// </summary>
        public string? CategoryDescription { get; set; }
    }
}