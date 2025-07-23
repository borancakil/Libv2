using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LibraryApp.Domain.Entities;

namespace LibraryApp.Domain.Interfaces
{
    /// <summary>
    /// Repository interface for Loan entity operations
    /// Provides abstraction layer for loan data access operations
    /// </summary>
    public interface ILoanRepository
    {
        /// <summary>
        /// Gets a loan by ID with optional navigation properties
        /// </summary>
        /// <param name="id">Loan ID</param>
        /// <param name="includeNavigationProperties">Whether to include related entities</param>
        /// <returns>Loan entity or null if not found</returns>
        Task<Loan?> GetByIdAsync(int id, bool includeNavigationProperties = false);

        /// <summary>
        /// Gets all loans by book ID
        /// </summary>
        /// <param name="bookId">Book ID</param>
        /// <param name="includeNavigationProperties">Whether to include related entities</param>
        /// <returns>Collection of loan entities</returns>
        Task<IEnumerable<Loan>> GetLoansByBookIdAsync(int bookId, bool includeNavigationProperties = false);

        /// <summary>
        /// Gets all loans by user ID
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="includeNavigationProperties">Whether to include related entities</param>
        /// <returns>Collection of loan entities</returns>
        Task<IEnumerable<Loan>> GetLoansByUserIdAsync(int userId, bool includeNavigationProperties = false);

        /// <summary>
        /// Gets active loan for specific book and user
        /// </summary>
        /// <param name="bookId">Book ID</param>
        /// <param name="userId">User ID</param>
        /// <returns>Active loan or null if not found</returns>
        Task<Loan?> GetActiveLoanAsync(int bookId, int userId);

        /// <summary>
        /// Gets all overdue loans
        /// </summary>
        /// <param name="includeNavigationProperties">Whether to include related entities</param>
        /// <returns>Collection of overdue loan entities</returns>
        Task<IEnumerable<Loan>> GetOverdueLoansAsync(bool includeNavigationProperties = false);

        /// <summary>
        /// Gets all active loans (not returned)
        /// </summary>
        /// <param name="includeNavigationProperties">Whether to include related entities</param>
        /// <returns>Collection of active loan entities</returns>
        Task<IEnumerable<Loan>> GetActiveLoansAsync(bool includeNavigationProperties = false);

        /// <summary>
        /// Adds a new loan to the repository
        /// </summary>
        /// <param name="loan">Loan entity to add</param>
        Task AddAsync(Loan loan);

        /// <summary>
        /// Updates an existing loan
        /// </summary>
        /// <param name="loan">Loan entity to update</param>
        void Update(Loan loan);

        /// <summary>
        /// Marks a loan for deletion
        /// </summary>
        /// <param name="loan">Loan entity to delete</param>
        void Delete(Loan loan);

        /// <summary>
        /// Saves all pending changes to the database
        /// </summary>
        /// <returns>Number of affected records</returns>
        Task<int> SaveChangesAsync();

        /// <summary>
        /// Checks if a loan exists by ID
        /// </summary>
        /// <param name="id">Loan ID</param>
        /// <returns>True if loan exists</returns>
        Task<bool> ExistsAsync(int id);
    }
}