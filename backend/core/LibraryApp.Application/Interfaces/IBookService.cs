using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryApp.Application.DTOs.Book;

namespace LibraryApp.Application.Interfaces
{
    /// <summary>
    /// Service interface for book-related business operations
    /// </summary>
    public interface IBookService
    {
        /// <summary>
        /// Creates a new book
        /// </summary>
        /// <param name="dto">Book creation data</param>
        /// <returns>The created book's ID</returns>
        Task<int> AddBookAsync(CreateBookDto dto);
        
        /// <summary>
        /// Gets a book by its ID
        /// </summary>
        /// <param name="id">Book ID</param>
        /// <returns>Book DTO or null if not found</returns>
        Task<BookDto?> GetByIdAsync(int id);
        
        /// <summary>
        /// Gets all books with optional filtering
        /// </summary>
        /// <param name="includeUnavailable">Whether to include unavailable books</param>
        /// <returns>Collection of book DTOs</returns>
        Task<IEnumerable<BookDto>> GetAllBooksAsync(bool includeUnavailable = true);
        
        /// <summary>
        /// Updates an existing book
        /// </summary>
        /// <param name="id">Book ID to update</param>
        /// <param name="dto">Updated book data</param>
        Task UpdateBookAsync(int id, UpdateBookDto dto);
        
        /// <summary>
        /// Processes a book borrowing operation
        /// </summary>
        /// <param name="dto">Borrowing data</param>
        Task BorrowBookAsync(BorrowBookDto dto);
        
        /// <summary>
        /// Deletes a book if it's available
        /// </summary>
        /// <param name="id">Book ID to delete</param>
        Task DeleteBookAsync(int id);
        
        /// <summary>
        /// Returns a borrowed book
        /// </summary>
        /// <param name="bookId">Book ID</param>
        /// <param name="userId">User ID who is returning</param>
        Task ReturnBookAsync(int bookId, int userId);
        
        /// <summary>
        /// Checks if a book exists
        /// </summary>
        /// <param name="id">Book ID</param>
        /// <returns>True if book exists</returns>
        Task<bool> BookExistsAsync(int id);
    }
}
