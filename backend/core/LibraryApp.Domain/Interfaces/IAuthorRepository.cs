using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryApp.Domain.Entities;


namespace LibraryApp.Domain.Interfaces
{
    /// <summary>
    /// Repository interface for Author entity operations
    /// </summary>
    public interface IAuthorRepository
    {
        /// <summary>
        /// Gets an author by their ID
        /// </summary>
        /// <param name="id">The author ID</param>
        /// <param name="includeNavigationProperties">Whether to include navigation properties</param>
        /// <returns>The author if found, null otherwise</returns>
        Task<Author?> GetByIdAsync(int id, bool includeNavigationProperties = false);

        /// <summary>
        /// Gets all authors
        /// </summary>
        /// <param name="includeNavigationProperties">Whether to include navigation properties</param>
        /// <returns>Collection of all authors</returns>
        Task<IEnumerable<Author>> GetAllAsync(bool includeNavigationProperties = false);

        /// <summary>
        /// Adds a new author
        /// </summary>
        /// <param name="author">The author to add</param>
        Task AddAsync(Author author);

        /// <summary>
        /// Updates an existing author
        /// </summary>
        /// <param name="author">The author to update</param>
        void Update(Author author);

        /// <summary>
        /// Deletes an author
        /// </summary>
        /// <param name="author">The author to delete</param>
        void Delete(Author author);

        /// <summary>
        /// Checks if an author exists by ID
        /// </summary>
        /// <param name="id">The author ID to check</param>
        /// <returns>True if exists, false otherwise</returns>
        Task<bool> ExistsAsync(int id);

        /// <summary>
        /// Saves all changes to the database
        /// </summary>
        Task<int> SaveChangesAsync();
    }
}
