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

            // Foreign Keys
            builder.Property(b => b.AuthorId)
                .IsRequired();

            builder.Property(b => b.PublisherId)
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

            builder.HasMany(b => b.BorrowedBooks)
                .WithOne(l => l.Book)
                .HasForeignKey(l => l.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            // Table Name
            builder.ToTable("Books");
        }
    }
} 