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
        /// Gets all books optimized for list view (without heavy collections)
        /// </summary>
        /// <returns>Collection of book entities optimized for listing</returns>
        IQueryable<Book> GetAllForList();
        
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

        /// <summary>
        /// Gets book by ID with optimized loading for detail view
        /// </summary>
        /// <param name="id">Book ID</param>
        /// <returns>Book entity optimized for detail view</returns>
        Task<Book?> GetByIdForDetailAsync(int id);

        /// <summary>
        /// Gets the count of borrowed books for a specific book
        /// </summary>
        /// <param name="bookId">Book ID</param>
        /// <returns>Count of borrowed books</returns>
        Task<int> GetBorrowedBooksCountAsync(int bookId);

        /// <summary>
        /// Gets the count of borrowed books for multiple books
        /// </summary>
        /// <param name="bookIds">List of book IDs</param>
        /// <returns>Dictionary of book IDs with their borrow counts</returns>
        Task<Dictionary<int, int>> GetBorrowedBooksCountAsync(List<int> bookIds);

        /// <summary>
        /// Gets the count of books for a specific author
        /// </summary>
        /// <param name="authorId">Author ID</param>
        /// <returns>Count of books by the author</returns>
        Task<int> GetBookCountByAuthorAsync(int authorId);

        /// <summary>
        /// Gets the count of books for a specific publisher
        /// </summary>
        /// <param name="publisherId">Publisher ID</param>
        /// <returns>Count of books by the publisher</returns>
        Task<int> GetBookCountByPublisherAsync(int publisherId);
    }
}