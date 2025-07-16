using Microsoft.EntityFrameworkCore;
using LibraryApp.Domain.Entities;

namespace LibraryApp.Persistence.Data
{
    public class LibraryDbContext : DbContext
    {
        // This constructor is needed for Dependency Injection in the API project.
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options)
        {
        }

        public DbSet<Book> Books { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Publisher> Publishers { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Loan> Loans { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(LibraryDbContext).Assembly);
        }
    }
}