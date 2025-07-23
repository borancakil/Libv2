using FluentValidation;
using LibraryApp.Application.DTOs.User;

namespace LibraryApp.Application.Validators.User
{
    /// <summary>
    /// Validator for AddUserDto using FluentValidation
    /// </summary>
    public class AddUserDtoValidator : AbstractValidator<AddUserDto>
    {
        public AddUserDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .Length(2, 100).WithMessage("Name must be between 2 and 100 characters")
                .Must(BeValidName).WithMessage("Name must contain only letters, spaces, and common punctuation");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format")
                .Length(5, 255).WithMessage("Email must be between 5 and 255 characters");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long")
                .Must(BeValidPassword).WithMessage("Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character");
        }

        private bool BeValidName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            // Allow letters, spaces, apostrophes, hyphens, and dots
            return name.All(c => char.IsLetter(c) || c == ' ' || c == '\'' || c == '-' || c == '.');
        }

        private bool BeValidPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return false;

            bool hasUpper = password.Any(char.IsUpper);
            bool hasLower = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);
            bool hasSpecial = password.Any(c => !char.IsLetterOrDigit(c));

            return hasUpper && hasLower && hasDigit && hasSpecial;
        }
    }
} 