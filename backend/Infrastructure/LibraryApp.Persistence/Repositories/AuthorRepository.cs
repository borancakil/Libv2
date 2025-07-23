using LibraryApp.Domain.Entities;
using LibraryApp.Domain.Interfaces;
using LibraryApp.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryApp.Persistence.Repositories
{
    public class AuthorRepository : IAuthorRepository
    {
        private readonly LibraryDbContext _context;

        public AuthorRepository(LibraryDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Author?> GetByIdAsync(int id, bool includeNavigationProperties = false)
        {
            if (id <= 0)
                return null;

            var query = _context.Authors.AsQueryable();
            
            if (includeNavigationProperties)
            {
                query = query.Include(a => a.Books);
            }

            return await query.FirstOrDefaultAsync(a => a.AuthorId == id);
        }

        public async Task<IEnumerable<Author>> GetAllAsync(bool includeNavigationProperties = false)
        {
            var query = _context.Authors.AsQueryable();
            
            if (includeNavigationProperties)
            {
                query = query.Include(a => a.Books);
            }

            return await query.ToListAsync();
        }

        public async Task AddAsync(Author author)
        {
            if (author == null)
                throw new ArgumentNullException(nameof(author));

            await _context.Authors.AddAsync(author);
        }

        public void Update(Author author)
        {
            if (author == null)
                throw new ArgumentNullException(nameof(author));

            _context.Authors.Update(author);
        }

        public void Delete(Author author)
        {
            if (author == null)
                throw new ArgumentNullException(nameof(author));

            _context.Authors.Remove(author);
        }

        public async Task<bool> ExistsAsync(int id)
        {
            Console.WriteLine($"👤 AUTHOR REPO DEBUG - ExistsAsync called with ID: {id}");
            
            if (id <= 0)
            {
                Console.WriteLine($"👤 AUTHOR REPO DEBUG - ID {id} is invalid, returning false");
                return false;
            }

            try
            {
                Console.WriteLine($"👤 AUTHOR REPO DEBUG - Querying database for Author ID: {id}");
                var exists = await _context.Authors.AnyAsync(a => a.AuthorId == id);
                Console.WriteLine($"👤 AUTHOR REPO DEBUG - Query result: {exists}");
                
                // Let's also check how many authors we have total
                var totalAuthors = await _context.Authors.CountAsync();
                Console.WriteLine($"👤 AUTHOR REPO DEBUG - Total authors in database: {totalAuthors}");
                
                return exists;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"👤 AUTHOR REPO DEBUG - EXCEPTION: {ex.Message}");
                Console.WriteLine($"👤 AUTHOR REPO DEBUG - EXCEPTION Type: {ex.GetType().Name}");
                Console.WriteLine($"👤 AUTHOR REPO DEBUG - EXCEPTION StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
} 