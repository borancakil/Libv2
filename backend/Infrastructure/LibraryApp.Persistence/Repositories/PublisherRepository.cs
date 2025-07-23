using LibraryApp.Domain.Entities;
using LibraryApp.Domain.Interfaces;
using LibraryApp.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryApp.Persistence.Repositories
{
    public class PublisherRepository : IPublisherRepository
    {
        private readonly LibraryDbContext _context;

        public PublisherRepository(LibraryDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Publisher?> GetByIdAsync(int id, bool includeNavigationProperties = false)
        {
            if (id <= 0)
                return null;

            var query = _context.Publishers.AsQueryable();
            
            if (includeNavigationProperties)
            {
                query = query.Include(p => p.Books);
            }

            return await query.FirstOrDefaultAsync(p => p.PublisherId == id);
        }

        public async Task<IEnumerable<Publisher>> GetAllAsync(bool includeNavigationProperties = false)
        {
            var query = _context.Publishers.AsQueryable();
            
            if (includeNavigationProperties)
            {
                query = query.Include(p => p.Books);
            }

            return await query.ToListAsync();
        }

        public async Task AddAsync(Publisher publisher)
        {
            if (publisher == null)
                throw new ArgumentNullException(nameof(publisher));

            await _context.Publishers.AddAsync(publisher);
        }

        public void Update(Publisher publisher)
        {
            if (publisher == null)
                throw new ArgumentNullException(nameof(publisher));

            _context.Publishers.Update(publisher);
        }

        public void Delete(Publisher publisher)
        {
            if (publisher == null)
                throw new ArgumentNullException(nameof(publisher));

            _context.Publishers.Remove(publisher);
        }

        public async Task<bool> ExistsAsync(int id)
        {
            Console.WriteLine($"🏢 PUBLISHER REPO DEBUG - ExistsAsync called with ID: {id}");
            
            if (id <= 0)
            {
                Console.WriteLine($"🏢 PUBLISHER REPO DEBUG - ID {id} is invalid, returning false");
                return false;
            }

            try
            {
                Console.WriteLine($"🏢 PUBLISHER REPO DEBUG - Querying database for Publisher ID: {id}");
                var exists = await _context.Publishers.AnyAsync(p => p.PublisherId == id);
                Console.WriteLine($"🏢 PUBLISHER REPO DEBUG - Query result: {exists}");
                
                // Let's also check how many publishers we have total
                var totalPublishers = await _context.Publishers.CountAsync();
                Console.WriteLine($"🏢 PUBLISHER REPO DEBUG - Total publishers in database: {totalPublishers}");
                
                return exists;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🏢 PUBLISHER REPO DEBUG - EXCEPTION: {ex.Message}");
                Console.WriteLine($"🏢 PUBLISHER REPO DEBUG - EXCEPTION Type: {ex.GetType().Name}");
                Console.WriteLine($"🏢 PUBLISHER REPO DEBUG - EXCEPTION StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
} 