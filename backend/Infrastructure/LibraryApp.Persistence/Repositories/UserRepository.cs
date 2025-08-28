using LibraryApp.Domain.Entities;
using LibraryApp.Domain.Interfaces;
using LibraryApp.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LibraryApp.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly LibraryDbContext _context;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(LibraryDbContext context, ILogger<UserRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task AddAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            await _context.Users.AddAsync(user);
            
            _logger.LogInformation(
                "Database operation: {Operation} on {Table} - ID: {Id}, Email: {Email}, Name: {Name}", 
                "INSERT", "Users", user.UserId, user.Email, user.Name);
        }

        public async Task<User?> GetByEmailAsync(string email, bool includeNavigationProperties = false)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            var query = _context.Users.AsQueryable();

            if (includeNavigationProperties)
            {
                query = query.Include(u => u.BorrowedBooks)
                            .ThenInclude(loan => loan.Book)
                            .Include(u => u.FavoriteBooks)
                            .ThenInclude(fav => fav.Book);
            }

            return await query.FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant());
        }

        public async Task<User?> GetByIdAsync(int id, bool includeNavigationProperties = false)
        {
            if (id <= 0)
                return null;

            var query = _context.Users.AsQueryable();

            if (includeNavigationProperties)
            {
                query = query.Include(u => u.BorrowedBooks)
                            .ThenInclude(loan => loan.Book)
                            .Include(u => u.FavoriteBooks)
                            .ThenInclude(fav => fav.Book);
            }

            return await query.FirstOrDefaultAsync(u => u.UserId == id);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<User>> GetAllAsync(bool includeNavigationProperties = false)
        {
            var query = _context.Users.AsQueryable();

            if (includeNavigationProperties)
            {
                query = query.Include(u => u.BorrowedBooks)
                            .ThenInclude(loan => loan.Book)
                            .Include(u => u.FavoriteBooks)
                            .ThenInclude(fav => fav.Book);
            }

            return await query.ToListAsync();
        }

        public void Update(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            _context.Users.Update(user);
            
            _logger.LogInformation(
                "Database operation: {Operation} on {Table} - ID: {Id}, Email: {Email}, Name: {Name}", 
                "UPDATE", "Users", user.UserId, user.Email, user.Name);
        }

        public void Delete(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            _context.Users.Remove(user);
            
            _logger.LogInformation(
                "Database operation: {Operation} on {Table} - ID: {Id}, Email: {Email}, Name: {Name}", 
                "DELETE", "Users", user.UserId, user.Email, user.Name);
        }

        public async Task<bool> ExistsAsync(int id)
        {
            if (id <= 0)
                return false;

            return await _context.Users.AnyAsync(u => u.UserId == id);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            return await _context.Users.AnyAsync(u => u.Email == email.ToLowerInvariant());
        }

        public async Task<IEnumerable<Book>> GetBorrowedBooksAsync(int userId, bool includeNavigationProperties = false)
        {
            if (userId <= 0)
            {
                return Enumerable.Empty<Book>();
            }

            var query = _context.Loans.Where(l => l.UserId == userId && l.ReturnDate == null);

            if (includeNavigationProperties)
            {
                query = query.Include(l => l.Book!.Author)
                             .Include(l => l.Book!.Publisher);
            }

            var books = await query.Select(l => l.Book).ToListAsync();
            return books.Where(b => b != null).Cast<Book>();
        }

        public async Task<IEnumerable<Book>> GetFavoriteBooksAsync(int userId, bool includeNavigationProperties = false)
        {
            if (userId <= 0)
            {
                return Enumerable.Empty<Book>();
            }

            var query = _context.UserFavoriteBooks.Where(ufb => ufb.UserId == userId);

            if (includeNavigationProperties)
            {
                query = query.Include(ufb => ufb.Book!.Author)
                             .Include(ufb => ufb.Book!.Publisher);
            }

            var books = await query.Select(ufb => ufb.Book).ToListAsync();
            return books.Where(b => b != null).Cast<Book>();
        }

        public async Task<IEnumerable<User>> GetUsersWhoFavoritedAsync(int bookId, bool includeNavigationProperties = false)
        {
            if (bookId <= 0)
            {
                return Enumerable.Empty<User>();
            }

            var query = _context.UserFavoriteBooks
                .Where(ufb => ufb.BookId == bookId);

            if (includeNavigationProperties)
            {
                query = query.Include(ufb => ufb.User!)
                            .ThenInclude(u => u.BorrowedBooks)
                            .Include(ufb => ufb.User!)
                            .ThenInclude(u => u.FavoriteBooks);
            }

            var result = await query.Select(ufb => ufb.User!).ToListAsync();
            return result;
        }

        public async Task<IEnumerable<Loan>> GetAllLoansAsync()
        {
            return await _context.Loans
                .Include(l => l.Book)
                .Include(l => l.User)
                .ToListAsync();
        }

        public async Task AddFavoriteBookAsync(UserFavoriteBook userFavoriteBook)
        {
            await _context.UserFavoriteBooks.AddAsync(userFavoriteBook);
        }

        public Task RemoveFavoriteBookAsync(UserFavoriteBook userFavoriteBook)
        {
            _context.UserFavoriteBooks.Remove(userFavoriteBook);
            return Task.CompletedTask;
        }

        public async Task<UserFavoriteBook?> GetFavoriteBookAsync(int userId, int bookId)
        {
            if (userId <= 0 || bookId <= 0)
                return null;

            return await _context.UserFavoriteBooks
                .FirstOrDefaultAsync(ufb => ufb.UserId == userId && ufb.BookId == bookId);
        }
    }
}