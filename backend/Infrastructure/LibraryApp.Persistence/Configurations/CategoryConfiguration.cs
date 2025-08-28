using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LibraryApp.Domain.Entities;

namespace LibraryApp.Persistence.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            // Primary Key
            builder.HasKey(c => c.CategoryId);

            // Properties
            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.Description)
                .HasMaxLength(500)
                .IsRequired(false);

            // Relationships
            builder.HasMany(c => c.Books)
                .WithOne(b => b.Category)
                .HasForeignKey(b => b.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Table Name
            builder.ToTable("Categories");

            // Seed data - using anonymous objects for EF Core seed data
            builder.HasData(
                new { CategoryId = 1, Name = "Fiction", Description = "Fictional literature including novels, short stories, and poetry" },
                new { CategoryId = 2, Name = "Non-Fiction", Description = "Factual literature including biographies, history, and science" },
                new { CategoryId = 3, Name = "Educational", Description = "Educational materials including textbooks and reference books" },
                new { CategoryId = 4, Name = "Science Fiction", Description = "Speculative fiction with scientific and technological themes" },
                new { CategoryId = 5, Name = "Mystery", Description = "Detective and crime fiction" },
                new { CategoryId = 6, Name = "Romance", Description = "Love stories and romantic fiction" },
                new { CategoryId = 7, Name = "Fantasy", Description = "Imaginative fiction with magical and supernatural elements" },
                new { CategoryId = 8, Name = "Biography", Description = "Life stories of real people" },
                new { CategoryId = 9, Name = "History", Description = "Historical accounts and analysis" },
                new { CategoryId = 10, Name = "Science", Description = "Scientific literature and research" }
            );
        }
    }
} 