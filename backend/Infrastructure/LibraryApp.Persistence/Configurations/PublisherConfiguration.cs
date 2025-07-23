using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LibraryApp.Domain.Entities;

namespace LibraryApp.Persistence.Configurations
{
    public class PublisherConfiguration : IEntityTypeConfiguration<Publisher>
    {
        public void Configure(EntityTypeBuilder<Publisher> builder)
        {
            // Primary Key
            builder.HasKey(p => p.PublisherId);

            // Properties
            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(p => p.Address)
                .HasMaxLength(500);

            builder.Property(p => p.Website)
                .HasMaxLength(255);

            builder.Property(p => p.ContactEmail)
                .HasMaxLength(255);

            // Relationships
            builder.HasMany(p => p.Books)
                .WithOne(b => b.Publisher)
                .HasForeignKey(b => b.PublisherId)
                .OnDelete(DeleteBehavior.Restrict);

            // Table Name
            builder.ToTable("Publishers");
        }
    }
} 