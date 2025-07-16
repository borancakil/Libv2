// Önceki using'lere ek olarak BCrypt'i de ekleyelim
using BCrypt.Net;
using MediatR;
using LibraryApp.Domain.Interfaces;
using AutoMapper;
using FluentValidation;
using System.Threading;
using System.Threading.Tasks;
using LibraryApp.Domain.Entities;

// Namespace'i yeni klasör adıyla güncelle
namespace LibraryApp.Application.Features.Users.Commands.Register
{
    // Sınıf adını güncelle
    public class UserRegisterCommand : IRequest<int>
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }

    // Sınıf adını güncelle
    public class UserRegisterCommandHandler : IRequestHandler<UserRegisterCommand, int>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserRegisterCommandHandler(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<int> Handle(UserRegisterCommand request, CancellationToken cancellationToken)
        {
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
            {
                // Throw a specific validation error
                throw new ValidationException("User with this email already exists.");
            }

            var userEntity = _mapper.Map<User>(request);

            userEntity.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);

            await _userRepository.AddAsync(userEntity);
            await _userRepository.SaveChangesAsync();

            return userEntity.UserId;
        }
    }

    public class UserRegisterCommandValidator : AbstractValidator<UserRegisterCommand>
    {
        public UserRegisterCommandValidator()
        {
            RuleFor(p => p.Name).NotEmpty().MaximumLength(100);
            RuleFor(p => p.Email).NotEmpty().EmailAddress();
            RuleFor(p => p.Password).NotEmpty().MinimumLength(6);
        }
    }
}