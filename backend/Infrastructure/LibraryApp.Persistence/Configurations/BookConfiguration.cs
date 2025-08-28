using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LibraryApp.Domain.Entities;

namespace LibraryApp.Persistence.Configurations
{
    public class BookConfiguration : IEntityTypeConfiguration<Book>
    {
        public void Configure(EntityTypeBuilder<Book> builder)
        {
            // Primary Key
            builder.HasKey(b => b.BookId);

            // Properties
            builder.Property(b => b.Title)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(b => b.PublicationYear)
                .IsRequired();

            builder.Property(b => b.IsAvailable)
                .IsRequired()
                .HasDefaultValue(true);

            // Add unique constraint to prevent race conditions when borrowing
            // This ensures only one user can borrow a book at a time
            builder.HasIndex(b => new { b.BookId, b.IsAvailable })
                .HasFilter("[IsAvailable] = 1")
                .IsUnique();

            // Photo/Image properties
            builder.Property(b => b.CoverImage)
                .HasColumnType("VARBINARY(MAX)")
                .IsRequired(false);

            builder.Property(b => b.ImageContentType)
                .HasMaxLength(100)
                .IsRequired(false);

            builder.Property(b => b.ImageFileName)
                .HasMaxLength(255)
                .IsRequired(false);

            // Foreign Keys
            builder.Property(b => b.AuthorId)
                .IsRequired();

            builder.Property(b => b.PublisherId)
                .IsRequired();

            // Rating property
            builder.Property(b => b.Rating)
                .HasColumnType("DECIMAL(3,2)")
                .IsRequired()
                .HasDefaultValue(0);

            // Category relationship
            builder.Property(b => b.CategoryId)
                .IsRequired();

            // Relationships
            builder.HasOne(b => b.Author)
                .WithMany(a => a.Books)
                .HasForeignKey(b => b.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(b => b.Publisher)
                .WithMany(p => p.Books)
                .HasForeignKey(b => b.PublisherId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(b => b.Category)
                .WithMany(c => c.Books)
                .HasForeignKey(b => b.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(b => b.BorrowedBooks)
                .WithOne(l => l.Book)
                .HasForeignKey(l => l.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(b => b.FavoritedByUsers)
                .WithOne(ufb => ufb.Book)
                .HasForeignKey(ufb => ufb.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            // Table Name
            builder.ToTable("Books");
        }
    }
} 