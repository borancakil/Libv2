using System;

namespace LibraryApp.Application.DTOs.Book
{
    /// <summary>
    /// DTO for borrowing a book
    /// Only FluentValidation is used for validation
    /// </summary>
    public class BorrowBookDto
    {
        public int BookId { get; set; }
        public int UserId { get; set; }
        public DateTime BorrowDate { get; set; } = DateTime.Now;
        public DateTime DueDate { get; set; } = DateTime.Now.AddDays(14);

        public bool IsValid(out string validationError)
        {
            if (BookId <= 0)
            {
                validationError = "BookId must be greater than 0.";
                return false;
            }

            if (UserId <= 0)
            {
                validationError = "UserId must be greater than 0.";
                return false;
            }

            if (DueDate <= BorrowDate)
            {
                validationError = "DueDate must be after BorrowDate.";
                return false;
            }

            validationError = string.Empty;
            return true;
        }
    }
}