using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibraryApp.Application.DTOs.User;
using LibraryApp.Application.Exceptions;
using LibraryApp.Application.Helpers;
using LibraryApp.Application.Interfaces;
using LibraryApp.Domain.Entities;
using LibraryApp.Domain.Interfaces;

namespace LibraryApp.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;

        public UserService(IUserRepository userRepository, IJwtService jwtService)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
        }

        public async Task<int> AddUserAsync(AddUserDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
            if (existingUser != null)
                throw new DuplicateEmailException(dto.Email);

            var user = new User(dto.Name, dto.Email, PasswordHasher.Hash(dto.Password));

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();
            
            return user.UserId;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync(bool includeLoans = false)
        {
            var users = await _userRepository.GetAllAsync(includeLoans);

            return users.Select(u => new UserDto
            {
                UserId = u.UserId,
                Name = u.Name,
                Email = u.Email,
                Role = u.Role,
                RegistrationDate = u.RegistrationDate,
                ActiveLoansCount = includeLoans ? u.GetActiveLoans().Count() : 0,
                HasOverdueBooks = includeLoans ? u.HasOverdueBooks() : false
            });
        }

        public async Task<UserDto?> GetByIdAsync(int id)
        {
            if (id <= 0)
                return null;

            var user = await _userRepository.GetByIdAsync(id, includeNavigationProperties: true);
            if (user == null) 
                return null;

            return new UserDto
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                RegistrationDate = user.RegistrationDate,
                ActiveLoansCount = user.GetActiveLoans().Count(),
                HasOverdueBooks = user.HasOverdueBooks()
            };
        }

        public async Task<UserDto?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            var user = await _userRepository.GetByEmailAsync(email, includeNavigationProperties: true);
            if (user == null) 
                return null;

            return new UserDto
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                RegistrationDate = user.RegistrationDate,
                ActiveLoansCount = user.GetActiveLoans().Count(),
                HasOverdueBooks = user.HasOverdueBooks()
            };
        }

        public async Task UpdateUserAsync(int id, UpdateUserDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (id <= 0)
                throw new ArgumentException("User ID must be greater than zero.", nameof(id));

            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                throw new UserNotFoundException(id);

            // Check if email is being changed and if new email already exists
            if (!string.Equals(user.Email, dto.Email, StringComparison.OrdinalIgnoreCase))
            {
                if (await _userRepository.EmailExistsAsync(dto.Email))
                    throw new DuplicateEmailException(dto.Email);
            }

            // Update user properties using domain methods
            user.UpdateProfile(dto.Name, dto.Email, dto.Age, dto.Gender, dto.Address);

            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();
        }

        public async Task<string> LoginAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required", nameof(email));
            
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password is required", nameof(password));

            var user = await _userRepository.GetByEmailAsync(email);

            if (user == null)
                throw new UserNotFoundException(email);

            if (!PasswordHasher.Verify(password, user.Password))
                throw new UnauthorizedAccessException("Invalid email or password");

            return _jwtService.GenerateToken(user.UserId, user.Email, user.Role.ToString());
        }

        public async Task UpdatePasswordAsync(int userId, UpdatePasswordUserDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new UserNotFoundException(userId);

            var isMatch = PasswordHasher.Verify(dto.CurrentPassword, user.Password);
            if (!isMatch)
                throw new UnauthorizedAccessException("Current password is incorrect");

            // Use domain logic for password update
            try
            {
                user.UpdatePassword(PasswordHasher.Hash(dto.NewPassword));
                
                _userRepository.Update(user);
                await _userRepository.SaveChangesAsync();
            }
            catch (ArgumentException ex)
            {
                throw new ValidationException(ex.ParamName ?? "Password", ex.Message);
            }
        }

        public async Task PromoteUserToAdminAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new UserNotFoundException(userId);

            user.PromoteToAdmin();
            
            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();
        }

        public async Task DemoteUserToRegularAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new UserNotFoundException(userId);

            user.DemoteToUser();
            
            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId, includeNavigationProperties: true);
            if (user == null)
                throw new UserNotFoundException(userId);

            // Business rule: Cannot delete user with active loans
            if (user.GetActiveLoans().Any())
                throw new InvalidOperationException($"Cannot delete user '{user.Name}' because they have active book loans");

            _userRepository.Delete(user);
            await _userRepository.SaveChangesAsync();
        }

        public async Task<bool> UserExistsAsync(int userId)
        {
            return await _userRepository.ExistsAsync(userId);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _userRepository.EmailExistsAsync(email);
        }
    }
}
