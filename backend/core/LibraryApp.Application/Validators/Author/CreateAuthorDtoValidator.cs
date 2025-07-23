using FluentValidation;
using LibraryApp.Application.DTOs.Author;

namespace LibraryApp.Application.Validators.Author
{
    public class CreateAuthorDtoValidator : AbstractValidator<CreateAuthorDto>
    {
        public CreateAuthorDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Author name is required.")
                .Length(2, 100).WithMessage("Author name must be between 2 and 100 characters.");

            RuleFor(x => x.Biography)
                .MaximumLength(500).WithMessage("Biography cannot exceed 500 characters.")
                .When(x => !string.IsNullOrEmpty(x.Biography));

            RuleFor(x => x.Nationality)
                .MaximumLength(50).WithMessage("Nationality cannot exceed 50 characters.")
                .When(x => !string.IsNullOrEmpty(x.Nationality));
        }
    }
} 