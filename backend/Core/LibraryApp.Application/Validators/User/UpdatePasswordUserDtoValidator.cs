using FluentValidation;
using LibraryApp.Application.DTOs.User;

namespace LibraryApp.Application.Validators.User
{
    /// <summary>
    /// Validator for UpdatePasswordUserDto using FluentValidation
    /// </summary>
    public class UpdatePasswordUserDtoValidator : AbstractValidator<UpdatePasswordUserDto>
    {
        public UpdatePasswordUserDtoValidator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("Current password is required");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("New password is required")
                .MinimumLength(8).WithMessage("New password must be at least 8 characters long")
                .Must(BeValidPassword).WithMessage("New password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character");

            RuleFor(x => x)
                .Must(HaveDifferentPasswords)
                .WithMessage("New password must be different from current password");
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

        private bool HaveDifferentPasswords(UpdatePasswordUserDto dto)
        {
            return dto.CurrentPassword != dto.NewPassword;
        }
    }
} 