using LibraryApp.Domain.Entities;
using LibraryApp.Domain.Interfaces;
using LibraryApp.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryApp.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly LibraryDbContext _context;

        public UserRepository(LibraryDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task AddAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            await _context.Users.AddAsync(user);
        }

        public async Task<User?> GetByEmailAsync(string email, bool includeNavigationProperties = false)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            var query = _context.Users.AsQueryable();

            if (includeNavigationProperties)
            {
                query = query.Include(u => u.BorrowedBooks)
                            .ThenInclude(loan => loan.Book);
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
                            .ThenInclude(loan => loan.Book);
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
                            .ThenInclude(loan => loan.Book);
            }

            return await query.ToListAsync();
        }

        public void Update(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            _context.Users.Update(user);
        }

        public void Delete(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            _context.Users.Remove(user);
            // Note: SaveChangesAsync() should be called by the service layer
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
    }
}