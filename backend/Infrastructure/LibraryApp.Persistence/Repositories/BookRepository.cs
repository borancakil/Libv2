using LibraryApp.Domain.Entities;
using LibraryApp.Domain.Interfaces; 
using LibraryApp.Persistence.Data;  
using Microsoft.EntityFrameworkCore;

namespace LibraryApp.Persistence.Repositories
{
    public class BookRepository : IBookRepository
    {
        private readonly LibraryDbContext _context;

        public BookRepository(LibraryDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task AddAsync(Book book)
        {
            if (book == null)
                throw new ArgumentNullException(nameof(book));

            await _context.Books.AddAsync(book);
        }

        public async Task<IEnumerable<Book>> GetAllAsync(bool includeNavigationProperties = false)
        {
            var query = _context.Books.AsQueryable();

            if (includeNavigationProperties)
            {
                query = query
                    .Include(b => b.Author)
                    .Include(b => b.Publisher)
                    .Include(b => b.BorrowedBooks);
            }

            return await query.ToListAsync();
        }

        public async Task<Book?> GetByIdAsync(int id, bool includeNavigationProperties = false)
        {
            if (id <= 0)
                return null;

            var query = _context.Books.AsQueryable();

            if (includeNavigationProperties)
            {
                query = query
                    .Include(b => b.Author)
                    .Include(b => b.Publisher)
                    .Include(b => b.BorrowedBooks);
            }

            return await query.FirstOrDefaultAsync(b => b.BookId == id);
        }

        public async Task<int> SaveChangesAsync()
        {
            Console.WriteLine("📖 BOOK REPO DEBUG - SaveChangesAsync called");
            
            try
            {
                Console.WriteLine("📖 BOOK REPO DEBUG - Calling _context.SaveChangesAsync()");
                var result = await _context.SaveChangesAsync();
                Console.WriteLine($"📖 BOOK REPO DEBUG - SaveChangesAsync completed successfully. Changes saved: {result}");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"📖 BOOK REPO DEBUG - EXCEPTION in SaveChangesAsync: {ex.Message}");
                Console.WriteLine($"📖 BOOK REPO DEBUG - EXCEPTION Type: {ex.GetType().Name}");
                Console.WriteLine($"📖 BOOK REPO DEBUG - EXCEPTION StackTrace: {ex.StackTrace}");
                Console.WriteLine($"📖 BOOK REPO DEBUG - Inner Exception: {ex.InnerException?.Message}");
                throw;
            }
        }

        public void Update(Book book)
        {
            if (book == null)
                throw new ArgumentNullException(nameof(book));

            _context.Books.Update(book);
        }

        public void Delete(Book book)
        {
            if (book == null)
                throw new ArgumentNullException(nameof(book));

            _context.Books.Remove(book);
            // Note: SaveChangesAsync() should be called by the service layer
        }

        public async Task<bool> ExistsAsync(int id)
        {
            if (id <= 0)
                return false;

            return await _context.Books.AnyAsync(b => b.BookId == id);
        }
    }
}