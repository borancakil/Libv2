using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LibraryApp.Domain.Entities;

namespace LibraryApp.Persistence.Configurations
{
    public class UserFavoriteBookConfiguration : IEntityTypeConfiguration<UserFavoriteBook>
    {
        public void Configure(EntityTypeBuilder<UserFavoriteBook> builder)
        {
            // Configure composite primary key
            builder.HasKey(ufb => new { ufb.UserId, ufb.BookId });

            // Configure relationships
            builder.HasOne(ufb => ufb.User)
                   .WithMany(u => u.FavoriteBooks)
                   .HasForeignKey(ufb => ufb.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ufb => ufb.Book)
                   .WithMany(b => b.FavoritedByUsers)
                   .HasForeignKey(ufb => ufb.BookId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Configure table name
            builder.ToTable("UserFavoriteBooks");

            // Configure properties
            builder.Property(ufb => ufb.AddedDate)
                   .IsRequired();
        }
    }
} 