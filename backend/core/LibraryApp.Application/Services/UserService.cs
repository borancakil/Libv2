using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibraryApp.Application.DTOs.User;
using LibraryApp.Application.DTOs.Book;
using LibraryApp.Application.Exceptions;
using LibraryApp.Application.Helpers;
using LibraryApp.Application.Interfaces;
using LibraryApp.Domain.Entities;
using LibraryApp.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using AutoMapper;

namespace LibraryApp.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IBookRepository _bookRepository;
        private readonly IJwtService _jwtService;
        private readonly ILogger<UserService> _logger;
        private readonly IMapper _mapper;

        public UserService(
            IUserRepository userRepository, 
            IBookRepository bookRepository, 
            IJwtService jwtService, 
            ILogger<UserService> logger,
            IMapper mapper)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _bookRepository = bookRepository ?? throw new ArgumentNullException(nameof(bookRepository));
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<int> AddUserAsync(AddUserDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
            if (existingUser != null)
                throw new DuplicateEmailException(dto.Email);

            var user = new User(dto.Name, dto.Email, PasswordHasher.Hash(dto.Password));
            var a = _userRepository.GetAllAsync().Result.AsQueryable().Where(x => x.Email == "dfgdfg").ToList();
            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();
            
            // Yeni JWT servisi RSA key pair gerektirmez

            _logger.LogInformation(
                "User registered - ID: {UserId}, Name: {Name}, Email: {Email}, Role: {Role}", 
                user.UserId, dto.Name, dto.Email, user.Role);
            
            return user.UserId;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync(bool includeLoans = false)
        {
            var users = await _userRepository.GetAllAsync(includeLoans);

            var dtos = _mapper.Map<IEnumerable<UserDto>>(users);
            if (!includeLoans)
            {
                foreach (var dto in dtos)
                {
                    dto.ActiveLoansCount = 0;
                    dto.HasOverdueBooks = false;
                }
            }
            return dtos;
        }

        public async Task<UserDto?> GetByIdAsync(int id)
        {
            if (id <= 0)
                return null;

            var user = await _userRepository.GetByIdAsync(id, includeNavigationProperties: true);
            if (user == null) 
                return null;

            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            var user = await _userRepository.GetByEmailAsync(email, includeNavigationProperties: true);
            if (user == null) 
                return null;

            return _mapper.Map<UserDto>(user);
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

            _logger.LogInformation(
                "User profile updated - ID: {UserId}, Name: {Name}, Email: {Email}, Age: {Age}, Gender: {Gender}",
                id, dto.Name, dto.Email, dto.Age, dto.Gender);
        }

        public async Task<UserDto> LoginAsync(string email, string password)
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

            _logger.LogInformation(
                "User authenticated successfully - ID: {UserId}, Email: {Email}, Role: {Role}", 
                user.UserId, email, user.Role);

            return _mapper.Map<UserDto>(user);
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

                _logger.LogInformation(
                    "User password updated successfully - ID: {UserId}, Email: {Email}",
                    userId, user.Email);
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

            // Yeni JWT servisi RSA key pair gerektirmez, silme işlemi gerekmez

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

        public async Task<IEnumerable<BookDto>> GetBorrowedBooksAsync(int userId)
        {
            if (userId <= 0)
                throw new ArgumentException($"Invalid user ID: {userId}. User ID must be greater than zero.", nameof(userId));

            if (!await _userRepository.ExistsAsync(userId))
                throw new UserNotFoundException(userId);

            var borrowedBooks = await _userRepository.GetBorrowedBooksAsync(userId, includeNavigationProperties: true);

            var dtos = _mapper.Map<IEnumerable<BookDto>>(borrowedBooks);
            foreach (var dto in dtos)
                dto.BorrowCount = 0;
            return dtos;
        }

        public async Task<IEnumerable<BookDto>> GetFavoriteBooksAsync(int userId)
        {
            if (userId <= 0)
                throw new ArgumentException($"Invalid user ID: {userId}. User ID must be greater than zero.", nameof(userId));

            if (!await _userRepository.ExistsAsync(userId))
                throw new UserNotFoundException(userId);

            var favoriteBooks = await _userRepository.GetFavoriteBooksAsync(userId, includeNavigationProperties: true);

            var dtos = _mapper.Map<IEnumerable<BookDto>>(favoriteBooks);
            foreach (var dto in dtos)
                dto.BorrowCount = 0;
            return dtos;
        }

        public async Task AddToFavoritesAsync(int userId, int bookId)
        {
            if (userId <= 0)
                throw new ArgumentException("User ID must be greater than zero.", nameof(userId));

            if (bookId <= 0)
                throw new ArgumentException("Book ID must be greater than zero.", nameof(bookId));

            var user = await _userRepository.GetByIdAsync(userId, includeNavigationProperties: true);
            if (user == null)
                throw new UserNotFoundException(userId);

            // Check if book exists
            var book = await _bookRepository.GetByIdAsync(bookId);
            if (book == null)
                throw new BookNotFoundException(bookId);

            if (user.IsBookInFavorites(bookId))
                throw new InvalidOperationException($"Book {bookId} is already in favorites");

            var favoriteBook = new UserFavoriteBook(userId, bookId);
            await _userRepository.AddFavoriteBookAsync(favoriteBook);
            await _userRepository.SaveChangesAsync();
        }

        public async Task<bool> RemoveFromFavoritesAsync(int userId, int bookId)
        {
            if (userId <= 0)
                throw new ArgumentException("User ID must be greater than zero.", nameof(userId));

            if (bookId <= 0)
                throw new ArgumentException("Book ID must be greater than zero.", nameof(bookId));

            // Check if user exists
            if (!await _userRepository.ExistsAsync(userId))
                throw new UserNotFoundException(userId);

            // Check if book exists
            if (!await _bookRepository.ExistsAsync(bookId))
                throw new BookNotFoundException(bookId);

            // Check if the favorite relationship exists
            var favoriteBook = await _userRepository.GetFavoriteBookAsync(userId, bookId);
            if (favoriteBook == null)
            {
                return false; // Not in favorites
            }

            // Remove the favorite relationship
            await _userRepository.RemoveFavoriteBookAsync(favoriteBook);
            await _userRepository.SaveChangesAsync();
            return true; // Successfully removed
        }

        public async Task<IEnumerable<Loan>> GetAllLoansAsync()
        {
            try
            {
                return await _userRepository.GetAllLoansAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving all loans: {ex.Message}", ex);
            }
        }
    }
}
