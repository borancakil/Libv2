namespace LibraryApp.Domain.Entities
{
    /// <summary>
    /// Represents a book loan in the library system
    /// </summary>
    public class Loan
    {
        // Parameterless constructor for EF Core
        private Loan() { }

        public int LoanId { get; set; }

        public DateTime LoanDate { get; set; }

        public DateTime DueDate { get; set; }

        public DateTime? ReturnDate { get; set; }

        public int BookId { get; set; }

        public int UserId { get; set; }

        // Navigation properties
        public Book? Book { get; set; }
        public User? User { get; set; }

        // Public constructors
        public Loan(int bookId, int userId, DateTime loanDate, DateTime dueDate) : this()
        {
            if (bookId <= 0)
                throw new ArgumentException("Book ID must be a positive number.", nameof(bookId));
            
            if (userId <= 0)
                throw new ArgumentException("User ID must be a positive number.", nameof(userId));
            
            if (dueDate <= loanDate)
                throw new ArgumentException("Due date must be after loan date.", nameof(dueDate));
            
            if (loanDate.Date < DateTime.Today.AddDays(-1))
                throw new ArgumentException("Loan date cannot be more than 1 day in the past.", nameof(loanDate));

            BookId = bookId;
            UserId = userId;
            LoanDate = loanDate;
            DueDate = dueDate;
            ReturnDate = null; // Initially null (not returned)
        }

        public Loan(int bookId, int userId, DateTime dueDate) 
            : this(bookId, userId, DateTime.UtcNow, dueDate)
        {
        }

        // --- Domain Logic Methods ---

        /// <summary>
        /// Checks if the loan is currently active (not returned)
        /// </summary>
        /// <returns>True if loan is active</returns>
        public bool IsActive()
        {
            return ReturnDate == null;
        }

        /// <summary>
        /// Checks if the loan is overdue
        /// </summary>
        /// <returns>True if loan is overdue</returns>
        public bool IsOverdue()
        {
            return IsActive() && DateTime.UtcNow.Date > DueDate.Date;
        }

        /// <summary>
        /// Gets the number of days the loan is overdue (0 if not overdue)
        /// </summary>
        /// <returns>Number of overdue days</returns>
        public int GetOverdueDays()
        {
            if (!IsOverdue())
                return 0;

            var overdueDays = (DateTime.UtcNow.Date - DueDate.Date).Days;
            return Math.Max(0, overdueDays);
        }

        /// <summary>
        /// Marks the loan as returned
        /// </summary>
        /// <param name="returnDate">Return date (defaults to current time)</param>
        public void MarkAsReturned(DateTime? returnDate = null)
        {
            if (!IsActive())
                throw new InvalidOperationException("Cannot return a book that has already been returned.");

            ReturnDate = returnDate ?? DateTime.UtcNow;
        }

        /// <summary>
        /// Extends the due date for the loan
        /// </summary>
        /// <param name="newDueDate">New due date</param>
        public void ExtendDueDate(DateTime newDueDate)
        {
            if (!IsActive())
                throw new InvalidOperationException("Cannot extend due date for a returned book.");

            if (newDueDate <= DueDate)
                throw new ArgumentException("New due date must be after current due date.", nameof(newDueDate));

            if (newDueDate > DateTime.UtcNow.AddMonths(6))
                throw new ArgumentException("Due date cannot be extended more than 6 months from now.", nameof(newDueDate));

            DueDate = newDueDate;
        }

        /// <summary>
        /// Calculates the loan duration in days
        /// </summary>
        /// <returns>Loan duration in days</returns>
        public int GetLoanDurationDays()
        {
            var endDate = ReturnDate ?? DateTime.UtcNow;
            return (endDate.Date - LoanDate.Date).Days;
        }

        /// <summary>
        /// Validates if loan period is within acceptable limits
        /// </summary>
        /// <param name="maxLoanDays">Maximum allowed loan days</param>
        /// <returns>True if loan period is valid</returns>
        public static bool IsValidLoanPeriod(DateTime loanDate, DateTime dueDate, int maxLoanDays = 180)
        {
            var loanPeriod = (dueDate - loanDate).Days;
            return loanPeriod > 0 && loanPeriod <= maxLoanDays;
        }
    }
}