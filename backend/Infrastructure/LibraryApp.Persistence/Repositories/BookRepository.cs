using LibraryApp.Domain.Entities;
using LibraryApp.Domain.Interfaces; 
using LibraryApp.Persistence.Data;  
using Microsoft.EntityFrameworkCore;
using LibraryApp.Application.Interfaces;


namespace LibraryApp.Persistence.Repositories
{
    public class BookRepository : IBookRepository
    {
        private readonly LibraryDbContext _context;
        private readonly ILoggingService _loggingService;

        public BookRepository(LibraryDbContext context, ILoggingService loggingService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
        }

        public async Task AddAsync(Book book)
        {
            if (book == null)
                throw new ArgumentNullException(nameof(book));

            await _context.Books.AddAsync(book);
    
            _loggingService.LogDataOperation("INSERT", "Books", book.BookId, new { Title = book.Title });
        }

        public async Task<IEnumerable<Book>> GetAllAsync(bool includeNavigationProperties = false)
        {
            var query = _context.Books.AsQueryable();

            if (includeNavigationProperties)
            {
                query = query
                    .Include(b => b.Author)
                    .Include(b => b.Publisher)
                    .Include(b => b.Category);
                // BorrowedBooks koleksiyonunu yüklemek yerine sadece sayısını alacağız
            }

            return await query.ToListAsync();
        }

        public IQueryable<Book> GetAllForList()
        {
            return _context.Books
                .Include(b => b.Author)
                .Include(b => b.Publisher)
                .AsNoTracking();
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
                    .Include(b => b.Category);
                // BorrowedBooks koleksiyonunu yüklemek yerine sadece sayısını alacağız
            }

            return await query.FirstOrDefaultAsync(b => b.BookId == id);
        }

        /// <summary>
        /// Gets book by ID with optimized loading for detail view
        /// </summary>
        public async Task<Book?> GetByIdForDetailAsync(int id)
        {
            if (id <= 0)
                return null;

            var query = _context.Books
                .Include(b => b.Author)
                .Include(b => b.Publisher)
                .Include(b => b.Category)
                .Select(b => new Book(b.Title)
                {
                    BookId = b.BookId,
                    PublicationYear = b.PublicationYear,
                    Rating = b.Rating,
                    CategoryId = b.CategoryId,
                    Category = b.Category,
                    AuthorId = b.AuthorId,
                    Author = b.Author,
                    PublisherId = b.PublisherId,
                    Publisher = b.Publisher,
                    ImageContentType = b.ImageContentType,
                    ImageFileName = b.ImageFileName,
                    // CoverImage'ı yüklemeyeceğiz - sadece metadata
                    // BorrowedBooks'u yüklemeyeceğiz - sadece sayısını alacağız
                });

            return await query.FirstOrDefaultAsync(b => b.BookId == id);
        }

        /// <summary>
        /// Gets the count of borrowed books for a specific book
        /// </summary>
        public async Task<int> GetBorrowedBooksCountAsync(int bookId)
        {
            if (bookId <= 0)
                return 0;

            return await _context.Loans
                .Where(l => l.BookId == bookId)
                .CountAsync();
        }

        /// <summary>
        /// Gets the count of borrowed books for multiple books
        /// </summary>
        public async Task<Dictionary<int, int>> GetBorrowedBooksCountAsync(List<int> bookIds)
        {
            if (!bookIds.Any())
                return new Dictionary<int, int>();

            var counts = await _context.Loans
                .Where(l => bookIds.Contains(l.BookId))
                .GroupBy(l => l.BookId)
                .Select(g => new { BookId = g.Key, Count = g.Count() })
                .ToListAsync();

            return counts.ToDictionary(x => x.BookId, x => x.Count);
        }

        /// <summary>
        /// Gets the count of books for a specific author
        /// </summary>
        public async Task<int> GetBookCountByAuthorAsync(int authorId)
        {
            if (authorId <= 0)
                return 0;

            return await _context.Books
                .Where(b => b.AuthorId == authorId)
                .CountAsync();
        }

        /// <summary>
        /// Gets the count of books for a specific publisher
        /// </summary>
        public async Task<int> GetBookCountByPublisherAsync(int publisherId)
        {
            if (publisherId <= 0)
                return 0;

            return await _context.Books
                .Where(b => b.PublisherId == publisherId)
                .CountAsync();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Update(Book book)
        {
            if (book == null)
                throw new ArgumentNullException(nameof(book));

            _context.Books.Update(book);

            _loggingService.LogDataOperation("UPDATE", "Books", book.BookId, new { Title = book.Title });

        }

        public void Delete(Book book)
        {
            if (book == null)
                throw new ArgumentNullException(nameof(book));

            _context.Books.Remove(book);
            _loggingService.LogDataOperation("DELETE", "Books", book.BookId, new { Title = book.Title });
        }

        public async Task<bool> ExistsAsync(int id)
        {
            if (id <= 0)
                return false;

            return await _context.Books.AnyAsync(b => b.BookId == id);
        }
    }
}