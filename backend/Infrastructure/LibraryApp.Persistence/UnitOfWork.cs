using System;
using System.Threading.Tasks;
using LibraryApp.Domain.Interfaces;
using LibraryApp.Persistence.Data;
using LibraryApp.Persistence.Repositories;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace LibraryApp.Persistence
{
    /// <summary>
    /// Unit of Work implementation for managing transactions across multiple repositories
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly LibraryDbContext _context;
        private readonly ILogger<UnitOfWork> _logger;
        private IDbContextTransaction? _transaction;
        private bool _disposed = false;

        // Repository instances
        private IBookRepository? _books;
        private IUserRepository? _users;
        private ILoanRepository? _loans;
        private IAuthorRepository? _authors;
        private IPublisherRepository? _publishers;
        private ICategoryRepository? _categories;

        public UnitOfWork(LibraryDbContext context, ILogger<UnitOfWork> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IBookRepository Books => _books ??= new BookRepository(_context, null);
        public IUserRepository Users => _users ??= new UserRepository(_context, null);
        public ILoanRepository Loans => _loans ??= new LoanRepository(_context, null);
        public IAuthorRepository Authors => _authors ??= new AuthorRepository(_context, null);
        public IPublisherRepository Publishers => _publishers ??= new PublisherRepository(_context, null);
        public ICategoryRepository Categories => _categories ??= new CategoryRepository(_context);

        public async Task BeginTransactionAsync()
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException("Transaction already started");
            }

            _transaction = await _context.Database.BeginTransactionAsync();
            _logger.LogInformation("Database transaction started");
        }

        public async Task<int> SaveChangesAsync()
        {
            try
            {
                var result = await _context.SaveChangesAsync();
                _logger.LogInformation("Database changes saved. Affected rows: {AffectedRows}", result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving database changes");
                throw;
            }
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("No transaction to commit");
            }

            try
            {
                await _transaction.CommitAsync();
                _logger.LogInformation("Database transaction committed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error committing transaction");
                throw;
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("No transaction to rollback");
            }

            try
            {
                await _transaction.RollbackAsync();
                _logger.LogInformation("Database transaction rolled back");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rolling back transaction");
                throw;
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _transaction?.Dispose();
                _context?.Dispose();
                _disposed = true;
            }
        }
    }
}
