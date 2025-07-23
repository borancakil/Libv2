using LibraryApp.Domain.Entities;

namespace LibraryApp.Domain.Interfaces
{
    /// <summary>
    /// Repository interface for Publisher entity operations
    /// </summary>
    public interface IPublisherRepository
    {
        /// <summary>
        /// Gets a publisher by their ID
        /// </summary>
        /// <param name="id">The publisher ID</param>
        /// <param name="includeNavigationProperties">Whether to include navigation properties</param>
        /// <returns>The publisher if found, null otherwise</returns>
        Task<Publisher?> GetByIdAsync(int id, bool includeNavigationProperties = false);

        /// <summary>
        /// Gets all publishers
        /// </summary>
        /// <param name="includeNavigationProperties">Whether to include navigation properties</param>
        /// <returns>Collection of all publishers</returns>
        Task<IEnumerable<Publisher>> GetAllAsync(bool includeNavigationProperties = false);

        /// <summary>
        /// Adds a new publisher
        /// </summary>
        /// <param name="publisher">The publisher to add</param>
        Task AddAsync(Publisher publisher);

        /// <summary>
        /// Updates an existing publisher
        /// </summary>
        /// <param name="publisher">The publisher to update</param>
        void Update(Publisher publisher);

        /// <summary>
        /// Deletes a publisher
        /// </summary>
        /// <param name="publisher">The publisher to delete</param>
        void Delete(Publisher publisher);

        /// <summary>
        /// Checks if a publisher exists by ID
        /// </summary>
        /// <param name="id">The publisher ID to check</param>
        /// <returns>True if exists, false otherwise</returns>
        Task<bool> ExistsAsync(int id);

        /// <summary>
        /// Saves all changes to the database
        /// </summary>
        Task<int> SaveChangesAsync();
    }
} 