using FluentValidation;
using LibraryApp.Application.DTOs.User;

namespace LibraryApp.Application.Validators.User
{
    /// <summary>
    /// Validator for LoginUserDto using FluentValidation
    /// </summary>
    public class LoginUserDtoValidator : AbstractValidator<LoginUserDto>
    {
        public LoginUserDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(1).WithMessage("Password cannot be empty");
        }
    }
} 