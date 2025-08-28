using LibraryApp.Domain.Entities;
using LibraryApp.Domain.Interfaces;
using LibraryApp.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using LibraryApp.Application.Interfaces;

namespace LibraryApp.Persistence.Repositories
{
    public class LoanRepository : ILoanRepository
    {
        private readonly LibraryDbContext _context;
        private readonly ILoggingService _loggingService;

        public LoanRepository(LibraryDbContext context, ILoggingService loggingService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
        }

        public async Task AddAsync(Loan loan)
        {
            if (loan == null)
                throw new ArgumentNullException(nameof(loan));

            await _context.Loans.AddAsync(loan);

            _loggingService.LogDataOperation("INSERT", "Loans", loan.LoanId, new { UserId = loan.UserId, BookId = loan.BookId });
        }

        public async Task<Loan?> GetByIdAsync(int id, bool includeNavigationProperties = false)
        {
            if (id <= 0)
                return null;

            var query = _context.Loans.AsQueryable();

            if (includeNavigationProperties)
            {
                query = query.Include(l => l.Book)
                           .Include(l => l.User);
            }

            return await query.FirstOrDefaultAsync(l => l.LoanId == id);
        }

        public async Task<IEnumerable<Loan>> GetLoansByBookIdAsync(int bookId, bool includeNavigationProperties = false)
        {
            if (bookId <= 0)
                return Enumerable.Empty<Loan>();

            var query = _context.Loans.Where(l => l.BookId == bookId);

            if (includeNavigationProperties)
            {
                query = query.Include(l => l.Book)
                           .Include(l => l.User);
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Loan>> GetLoansByUserIdAsync(int userId, bool includeNavigationProperties = false)
        {
            if (userId <= 0)
                return Enumerable.Empty<Loan>();

            var query = _context.Loans.Where(l => l.UserId == userId);

            if (includeNavigationProperties)
            {
                query = query.Include(l => l.Book)
                           .Include(l => l.User);
            }

            return await query.ToListAsync();
        }

        public async Task<Loan?> GetActiveLoanAsync(int bookId, int userId)
        {
            if (bookId <= 0 || userId <= 0)
                return null;

            return await _context.Loans
                .FirstOrDefaultAsync(l => l.BookId == bookId && 
                                         l.UserId == userId && 
                                         l.ReturnDate == null);
        }

        public async Task<IEnumerable<Loan>> GetOverdueLoansAsync(bool includeNavigationProperties = false)
        {
            var today = DateTime.UtcNow.Date;
            var query = _context.Loans.Where(l => l.ReturnDate == null && l.DueDate.Date < today);

            if (includeNavigationProperties)
            {
                query = query.Include(l => l.Book)
                           .Include(l => l.User);
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Loan>> GetActiveLoansAsync(bool includeNavigationProperties = false)
        {
            var query = _context.Loans.Where(l => l.ReturnDate == null);

            if (includeNavigationProperties)
            {
                query = query.Include(l => l.Book)
                           .Include(l => l.User);
            }

            return await query.ToListAsync();
        }

        public void Update(Loan loan)
        {
            if (loan == null)
                throw new ArgumentNullException(nameof(loan));

            _context.Loans.Update(loan);

            _loggingService.LogDataOperation("UPDATE", "Loans", loan.LoanId, new { UserId = loan.UserId, BookId = loan.BookId });
        }

        public void Delete(Loan loan)
        {
            if (loan == null)
                throw new ArgumentNullException(nameof(loan));

            _context.Loans.Remove(loan);

            _loggingService.LogDataOperation("DELETE", "Loans", loan.LoanId, new
            {
                UserId = loan.UserId,
                BookId = loan.BookId,
                LoanDate = loan.LoanDate,
                DueDate = loan.DueDate
            });
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            if (id <= 0)
                return false;

            return await _context.Loans.AnyAsync(l => l.LoanId == id);
        }

        #region Deprecated Methods (for backwards compatibility)
        
        [Obsolete("Use Update() method instead. This method violates repository pattern by calling SaveChangesAsync.")]
        public async Task UpdateAsync(Loan loan)
        {
            Update(loan);
            await SaveChangesAsync();
        }

        [Obsolete("Use Delete() method instead. This method violates repository pattern by calling SaveChangesAsync.")]
        public async Task DeleteAsync(int id)
        {
            var loan = await GetByIdAsync(id);
            if (loan != null)
            {
                Delete(loan);
                await SaveChangesAsync();
            }
        }

        #endregion
    }
}