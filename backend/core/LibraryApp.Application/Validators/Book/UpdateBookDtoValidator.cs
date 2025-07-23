using FluentValidation;
using LibraryApp.Application.DTOs.Book;

namespace LibraryApp.Application.Validators.Book
{
    /// <summary>
    /// Validator for UpdateBookDto using FluentValidation
    /// </summary>
    public class UpdateBookDtoValidator : AbstractValidator<UpdateBookDto>
    {
        public UpdateBookDtoValidator()
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

            RuleFor(x => x.IsAvailable)
                .NotNull().WithMessage("IsAvailable field is required");
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