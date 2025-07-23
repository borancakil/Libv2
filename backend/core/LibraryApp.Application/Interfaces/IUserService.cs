using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryApp.Application.DTOs.User;

namespace LibraryApp.Application.Interfaces
{
    /// <summary>
    /// Service interface for user-related business operations
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Creates a new user
        /// </summary>
        /// <param name="dto">User creation data</param>
        /// <returns>The created user's ID</returns>
        Task<int> AddUserAsync(AddUserDto dto);
        
        /// <summary>
        /// Gets a user by ID
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>User DTO or null if not found</returns>
        Task<UserDto?> GetByIdAsync(int id);
        
        /// <summary>
        /// Gets a user by email address
        /// </summary>
        /// <param name="email">User email address</param>
        /// <returns>User DTO or null if not found</returns>
        Task<UserDto?> GetByEmailAsync(string email);
        
        /// <summary>
        /// Gets all users with optional loan information
        /// </summary>
        /// <param name="includeLoans">Whether to include loan information</param>
        /// <returns>Collection of user DTOs</returns>
        Task<IEnumerable<UserDto>> GetAllUsersAsync(bool includeLoans = false);
        
        /// <summary>
        /// Updates an existing user
        /// </summary>
        /// <param name="id">User ID to update</param>
        /// <param name="dto">Updated user data</param>
        Task UpdateUserAsync(int id, UpdateUserDto dto);
        
        /// <summary>
        /// Authenticates user and returns JWT token
        /// </summary>
        /// <param name="email">User email</param>
        /// <param name="password">User password</param>
        /// <returns>JWT token</returns>
        Task<string> LoginAsync(string email, string password);
        
        /// <summary>
        /// Updates user password
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="dto">Password update data</param>
        Task UpdatePasswordAsync(int userId, UpdatePasswordUserDto dto);
        
        /// <summary>
        /// Promotes user to admin role
        /// </summary>
        /// <param name="userId">User ID</param>
        Task PromoteUserToAdminAsync(int userId);
        
        /// <summary>
        /// Demotes user to regular user role
        /// </summary>
        /// <param name="userId">User ID</param>
        Task DemoteUserToRegularAsync(int userId);
        
        /// <summary>
        /// Deletes a user if they have no active loans
        /// </summary>
        /// <param name="userId">User ID to delete</param>
        Task DeleteUserAsync(int userId);
        
        /// <summary>
        /// Checks if a user exists
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>True if user exists</returns>
        Task<bool> UserExistsAsync(int userId);
        
        /// <summary>
        /// Checks if an email is already registered
        /// </summary>
        /// <param name="email">Email address</param>
        /// <returns>True if email exists</returns>
        Task<bool> EmailExistsAsync(string email);
    }
}
