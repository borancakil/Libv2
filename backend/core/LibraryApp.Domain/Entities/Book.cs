namespace LibraryApp.Domain.Entities
{
    /// <summary>
    /// Represents a single book in the library's catalog.
    /// This is a root entity in our domain.
    /// </summary>
    public class Book
    {
        // Parameterless constructor for EF Core
        private Book() 
        {
            IsAvailable = true; 
        }

        public int BookId { get; set; }
        
        private string _title = string.Empty;
        public string Title 
        { 
            get => _title;
            set 
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Title cannot be null or empty.", nameof(value));
                _title = value;
            }
        }
        
        public int PublicationYear { get; set; }
        public bool IsAvailable { get; private set; } = true;

        public int AuthorId { get; set; }
        public Author? Author { get; set; } 

        public int PublisherId { get; set; }
        public Publisher? Publisher { get; set; }

        public ICollection<Loan> BorrowedBooks { get; set; } = new List<Loan>();

        // Public constructor with validation
        public Book(string title)
        {
            Title = title; // This will trigger validation
        }

        public Book(string title, int publicationYear, int authorId, int publisherId) : this(title)
        {
            PublicationYear = publicationYear;
            AuthorId = authorId;
            PublisherId = publisherId;
            IsAvailable = true; 
        }

        // --- Domain Logic Methods ---

        /// <summary>
        /// Marks the book as borrowed, enforcing the business rule that only an available book can be borrowed.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when book is not available</exception>
        public void MarkAsBorrowed()
        {
            if (!IsAvailable)
            {
                throw new InvalidOperationException("Book is not available to be borrowed.");
            }
            IsAvailable = false;
        }

        /// <summary>
        /// Marks the book as returned and available.
        /// </summary>
        public void MarkAsReturned()
        {
            IsAvailable = true;
        }

        /// <summary>
        /// Sets the availability status of the book.
        /// This method is used for administrative purposes when updating book details.
        /// </summary>
        /// <param name="isAvailable">The new availability status</param>
        public void SetAvailability(bool isAvailable)
        {
            IsAvailable = isAvailable;
        }

        /// <summary>
        /// Validates if the book can be deleted (not currently on loan)
        /// </summary>
        /// <returns>True if book can be deleted</returns>
        public bool CanBeDeleted()
        {
            return IsAvailable;
        }

        /// <summary>
        /// Validates publication year
        /// </summary>
        /// <param name="year">Year to validate</param>
        /// <returns>True if year is valid</returns>
        public static bool IsValidPublicationYear(int year)
        {
            return year >= 1000 && year <= DateTime.Now.Year + 1;
        }
    }
}