using FluentValidation;
using MediatR;
using LibraryApp.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using LibraryApp.Domain.Enums; 
using System.ComponentModel.DataAnnotations; 

namespace LibraryApp.Application.Features.Users.Commands.UpdateUser
{
    /// <summary>
    /// Represents the command to update a user's role.
    /// </summary>
    public class UpdateUserRoleCommand : IRequest<Unit>
    {
        public int UserId { get; set; }
        public UserRole NewRole { get; set; }
    }

    /// <summary>
    /// Handles the logic for the UpdateUserRoleCommand.
    /// </summary>
    public class UpdateUserRoleCommandHandler : IRequestHandler<UpdateUserRoleCommand, Unit>
    {
        private readonly IUserRepository _userRepository;

        public UpdateUserRoleCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Unit> Handle(UpdateUserRoleCommand request, CancellationToken cancellationToken)
        {
            // 1. Retrieve the user from the database.
            var user = await _userRepository.GetByIdAsync(request.UserId);

            // 2. Check if the user exists.
            if (user == null)
            {
                throw new FluentValidation.ValidationException($"User with ID {request.UserId} not found.");
            }

            // 3. Apply business logic based on the new role.
            if (request.NewRole == UserRole.Admin)
            {
                user.PromoteToAdmin();
            }
            else
            {
                user.DemoteToUser();
            }

            // 4. Save the changes.
            await _userRepository.SaveChangesAsync();

            // 5. Return Unit to signal completion (MediatR pattern).
            return Unit.Value;
        }

    }

    /// <summary>
    /// Validates the incoming UpdateUserRoleCommand.
    /// </summary>
    public class UpdateUserRoleCommandValidator : AbstractValidator<UpdateUserRoleCommand>
    {
        public UpdateUserRoleCommandValidator()
        {
            RuleFor(v => v.UserId)
                .GreaterThan(0).WithMessage("A valid User ID must be provided.");

            RuleFor(v => v.NewRole)
                // IsInEnum() ensures that the provided value is a valid member of the UserRole enum.
                .IsInEnum().WithMessage("A valid user role must be provided.");
        }
    }
}