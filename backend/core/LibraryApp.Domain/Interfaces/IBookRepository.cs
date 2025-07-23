using LibraryApp.Domain.Entities;

namespace LibraryApp.Domain.Interfaces
{
    /// <summary>
    /// Represents the repository pattern for Book entity operations.
    /// Provides abstraction layer for data access operations.
    /// </summary>
    public interface IBookRepository
    {
        /// <summary>
        /// Gets a book by its ID with optional navigation properties
        /// </summary>
        /// <param name="id">Book ID</param>
        /// <param name="includeNavigationProperties">Whether to include related entities</param>
        /// <returns>Book entity or null if not found</returns>
        Task<Book?> GetByIdAsync(int id, bool includeNavigationProperties = false);
        
        /// <summary>
        /// Gets all books with optional navigation properties
        /// </summary>
        /// <param name="includeNavigationProperties">Whether to include related entities</param>
        /// <returns>Collection of book entities</returns>
        Task<IEnumerable<Book>> GetAllAsync(bool includeNavigationProperties = false);
        
        /// <summary>
        /// Adds a new book to the repository
        /// </summary>
        /// <param name="book">Book entity to add</param>
        Task AddAsync(Book book);
        
        /// <summary>
        /// Updates an existing book
        /// </summary>
        /// <param name="book">Book entity to update</param>
        void Update(Book book);
        
        /// <summary>
        /// Marks a book for deletion
        /// </summary>
        /// <param name="book">Book entity to delete</param>
        void Delete(Book book);
        
        /// <summary>
        /// Saves all pending changes to the database
        /// </summary>
        /// <returns>Number of affected records</returns>
        Task<int> SaveChangesAsync();
        
        /// <summary>
        /// Checks if a book exists by ID
        /// </summary>
        /// <param name="id">Book ID</param>
        /// <returns>True if book exists</returns>
        Task<bool> ExistsAsync(int id);
    }
}