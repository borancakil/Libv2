using FluentValidation;
using LibraryApp.Application.DTOs.Book;

namespace LibraryApp.Application.Validators.Book
{
    /// <summary>
    /// Validator for CreateBookDto using FluentValidation
    /// </summary>
    public class CreateBookDtoValidator : AbstractValidator<CreateBookDto>
    {
        public CreateBookDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required")
                .Length(1, 255).WithMessage("Title must be between 1 and 255 characters")
                .Must(BeValidTitle).WithMessage("Title cannot contain only whitespace or special characters");

            RuleFor(x => x.PublicationYear)
                .InclusiveBetween(1000, DateTime.Now.Year + 1)
                .WithMessage($"Publication year must be between 1000 and {DateTime.Now.Year + 1}");

            RuleFor(x => x.AuthorId)
                .GreaterThan(0).WithMessage("Author ID must be a positive number");

            RuleFor(x => x.PublisherId)
                .GreaterThan(0).WithMessage("Publisher ID must be a positive number");

            RuleFor(x => x.Rating)
                .InclusiveBetween(0, 5).WithMessage("Rating must be between 0 and 5");

            RuleFor(x => x.CategoryId)
                .InclusiveBetween(1, 3).WithMessage("Category ID must be between 1 and 3");
        }

        private bool BeValidTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return false;

            // Check if title contains at least one alphanumeric character
            return title.Any(char.IsLetterOrDigit);
        }
    }
} 