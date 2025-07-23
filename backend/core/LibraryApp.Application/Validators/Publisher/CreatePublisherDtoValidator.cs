using FluentValidation;
using LibraryApp.Application.DTOs.Publisher;

namespace LibraryApp.Application.Validators.Publisher
{
    public class CreatePublisherDtoValidator : AbstractValidator<CreatePublisherDto>
    {
        public CreatePublisherDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Publisher name is required.")
                .Length(2, 100).WithMessage("Publisher name must be between 2 and 100 characters.");

            RuleFor(x => x.Address)
                .MaximumLength(200).WithMessage("Address cannot exceed 200 characters.")
                .When(x => !string.IsNullOrEmpty(x.Address));

            RuleFor(x => x.Website)
                .Must(BeValidUrl).WithMessage("Website must be a valid URL.")
                .When(x => !string.IsNullOrEmpty(x.Website));

            RuleFor(x => x.ContactEmail)
                .EmailAddress().WithMessage("Contact email must be a valid email address.")
                .When(x => !string.IsNullOrEmpty(x.ContactEmail));
        }

        private bool BeValidUrl(string? url)
        {
            if (string.IsNullOrEmpty(url)) return true;
            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }
    }
} 