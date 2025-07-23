using LibraryApp.Domain.Entities;

namespace LibraryApp.Domain.Interfaces
{
    /// <summary>
    /// Repository interface for User entity operations
    /// Provides abstraction layer for user data access operations
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Gets a user by ID with optional navigation properties
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="includeNavigationProperties">Whether to include related entities</param>
        /// <returns>User entity or null if not found</returns>
        Task<User?> GetByIdAsync(int id, bool includeNavigationProperties = false);
        
        /// <summary>
        /// Gets a user by email address with optional navigation properties
        /// </summary>
        /// <param name="email">User email address</param>
        /// <param name="includeNavigationProperties">Whether to include related entities</param>
        /// <returns>User entity or null if not found</returns>
        Task<User?> GetByEmailAsync(string email, bool includeNavigationProperties = false);
        
        /// <summary>
        /// Gets all users with optional navigation properties
        /// </summary>
        /// <param name="includeNavigationProperties">Whether to include related entities</param>
        /// <returns>Collection of user entities</returns>
        Task<IEnumerable<User>> GetAllAsync(bool includeNavigationProperties = false);
        
        /// <summary>
        /// Adds a new user to the repository
        /// </summary>
        /// <param name="user">User entity to add</param>
        Task AddAsync(User user);
        
        /// <summary>
        /// Updates an existing user
        /// </summary>
        /// <param name="user">User entity to update</param>
        void Update(User user);
        
        /// <summary>
        /// Marks a user for deletion
        /// </summary>
        /// <param name="user">User entity to delete</param>
        void Delete(User user);
        
        /// <summary>
        /// Saves all pending changes to the database
        /// </summary>
        /// <returns>Number of affected records</returns>
        Task<int> SaveChangesAsync();
        
        /// <summary>
        /// Checks if a user exists by ID
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>True if user exists</returns>
        Task<bool> ExistsAsync(int id);
        
        /// <summary>
        /// Checks if a user exists by email
        /// </summary>
        /// <param name="email">User email address</param>
        /// <returns>True if user exists</returns>
        Task<bool> EmailExistsAsync(string email);
    }
}