using LibraryApp.Domain.Entities;
using LibraryApp.Domain.Interfaces;
using LibraryApp.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using LibraryApp.Application.Interfaces;

namespace LibraryApp.Persistence.Repositories
{
    public class PublisherRepository : IPublisherRepository
    {
        private readonly LibraryDbContext _context;
        private readonly ILoggingService _loggingService;

        public PublisherRepository(LibraryDbContext context, ILoggingService loggingService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
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
            
            // Logging ekle
            _loggingService.LogDataOperation("INSERT", "Publishers", publisher.PublisherId, new { 
                Name = publisher.Name,
                Address = publisher.Address 
            });
        }

        public void Update(Publisher publisher)
        {
            if (publisher == null)
                throw new ArgumentNullException(nameof(publisher));

            _context.Publishers.Update(publisher);
            
            // Logging ekle
            _loggingService.LogDataOperation("UPDATE", "Publishers", publisher.PublisherId, new { 
                Name = publisher.Name,
                Address = publisher.Address 
            });
        }

        public void Delete(Publisher publisher)
        {
            if (publisher == null)
                throw new ArgumentNullException(nameof(publisher));

            _context.Publishers.Remove(publisher);
            
            // Logging ekle
            _loggingService.LogDataOperation("DELETE", "Publishers", publisher.PublisherId, new { 
                Name = publisher.Name 
            });
        }

        public async Task<bool> ExistsAsync(int id)
        {
            if (id <= 0)
                return false;

            return await _context.Publishers.AnyAsync(p => p.PublisherId == id);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
} 