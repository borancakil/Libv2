namespace LibraryApp.Domain.Entities
{
    /// <summary>
    /// Represents a single book in the library's catalog.
    /// This is a root entity in our domain.
    /// </summary>
    public class Book
    {
        public int BookId { get; set; }
        public string Title { get; set; }
        public int PublicationYear { get; set; }
        public bool IsAvailable { get; private set; } = true;

        public int AuthorId { get; set; }
        public Author Author { get; set; } 

        public int PublisherId { get; set; }
        public Publisher Publisher { get; set; }

        /// <summary>
        /// A collection of loan records associated with this book.
        /// </summary>
        public ICollection<Loan> BorrowedBooks { get; set; } = new List<Loan>();



        public Book(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title is required", nameof(title));
            Title = title;
        }

        // --- Domain Logic Methods ---



        /// <summary>
        /// Marks the book as borrowed, enforcing the business rule that only an available book can be borrowed.
        /// </summary>
        public void MarkAsBorrowed()
        {
            if (!IsAvailable)
            {
                // In a real application, you might throw a custom DomainException here.
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
    }
}