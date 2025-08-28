using FluentValidation;
using LibraryApp.Application.DTOs.Book;

namespace LibraryApp.Application.Validators.Book
{
    /// <summary>
    /// Validator for BorrowBookDto using FluentValidation
    /// </summary>
    public class BorrowBookDtoValidator : AbstractValidator<BorrowBookDto>
    {
        public BorrowBookDtoValidator()
        {
            RuleFor(x => x.BookId)
                .GreaterThan(0).WithMessage("Book ID must be a positive number");

            RuleFor(x => x.UserId)
                .GreaterThan(0).WithMessage("User ID must be a positive number");

            RuleFor(x => x.BorrowDate)
                .GreaterThanOrEqualTo(DateTime.Today.AddDays(-1))
                .WithMessage("Borrow date cannot be more than 1 day in the past")
                .LessThanOrEqualTo(DateTime.Today.AddDays(7))
                .WithMessage("Borrow date cannot be more than 7 days in the future");

            RuleFor(x => x.DueDate)
                .GreaterThan(x => x.BorrowDate)
                .WithMessage("Due date must be after borrow date")
                .LessThanOrEqualTo(DateTime.Today.AddMonths(6))
                .WithMessage("Loan period cannot exceed 6 months");

            RuleFor(x => x)
                .Must(HaveValidLoanPeriod)
                .WithMessage("Loan period must be between 1 day and 6 months");
        }

        private bool HaveValidLoanPeriod(BorrowBookDto dto)
        {
            var loanPeriod = dto.DueDate - dto.BorrowDate;
            return loanPeriod.TotalDays >= 1 && loanPeriod.TotalDays <= 180; // 6 months ≈ 180 days
        }
    }
} 