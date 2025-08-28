using System;
using System.Threading.Tasks;

namespace LibraryApp.Domain.Interfaces
{
    /// <summary>
    /// Unit of Work pattern interface for managing transactions across multiple repositories
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Gets the book repository
        /// </summary>
        IBookRepository Books { get; }
        
        /// <summary>
        /// Gets the user repository
        /// </summary>
        IUserRepository Users { get; }
        
        /// <summary>
        /// Gets the loan repository
        /// </summary>
        ILoanRepository Loans { get; }
        
        /// <summary>
        /// Gets the author repository
        /// </summary>
        IAuthorRepository Authors { get; }
        
        /// <summary>
        /// Gets the publisher repository
        /// </summary>
        IPublisherRepository Publishers { get; }
        
        /// <summary>
        /// Gets the category repository
        /// </summary>
        ICategoryRepository Categories { get; }
        
        /// <summary>
        /// Begins a new transaction
        /// </summary>
        /// <returns>Task representing the async operation</returns>
        Task BeginTransactionAsync();
        
        /// <summary>
        /// Commits all changes in the current transaction
        /// </summary>
        /// <returns>Number of affected rows</returns>
        Task<int> SaveChangesAsync();
        
        /// <summary>
        /// Commits the current transaction
        /// </summary>
        /// <returns>Task representing the async operation</returns>
        Task CommitTransactionAsync();
        
        /// <summary>
        /// Rolls back the current transaction
        /// </summary>
        /// <returns>Task representing the async operation</returns>
        Task RollbackTransactionAsync();
    }
}
