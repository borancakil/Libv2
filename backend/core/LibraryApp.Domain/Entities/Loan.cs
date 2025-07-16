namespace LibraryApp.Domain.Entities
{
    /// <summary>
    /// Represents the act of a user borrowing a book.
    /// This entity links a User to a Book for a specific period.
    /// </summary>
    public class Loan
    {
        public int LoanId { get; set; }

        /// <summary>
        /// The date when the book was borrowed.
        /// </summary>
        public DateTime LoanDate { get; set; }

        /// <summary>
        /// The date when the book is expected to be returned.
        /// </summary>
        public DateTime DueDate { get; set; }

        /// <summary>
        /// The actual date the book was returned.
        /// Null if the book has not yet been returned.
        /// </summary>
        public DateTime? ReturnDate { get; set; }

        // --- Relationships (Foreign Keys) ---

        /// <summary>
        /// The ID of the book that was borrowed.
        /// </summary>
        public int BookId { get; set; }

        /// <summary>
        /// The ID of the user who borrowed the book.
        /// </summary>
        public int UserId { get; set; }


        // --- Navigation Properties ---
        // These properties allow easy access to the related entities.

        public Book Book { get; set; }
        public User User { get; set; }
    }
}