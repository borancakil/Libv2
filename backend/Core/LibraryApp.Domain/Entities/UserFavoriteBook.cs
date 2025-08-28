using System;

namespace LibraryApp.Domain.Entities
{
    /// <summary>
    /// Represents a user's favorite book relationship
    /// </summary>
    public class UserFavoriteBook
    {
        // Parameterless constructor for EF Core
        private UserFavoriteBook() { }

        public int UserId { get; set; }
        public int BookId { get; set; }
        public DateTime AddedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public User? User { get; set; }
        public Book? Book { get; set; }

        // Public constructor
        public UserFavoriteBook(int userId, int bookId)
        {
            if (userId <= 0)
                throw new ArgumentException("User ID must be a positive number.", nameof(userId));
            
            if (bookId <= 0)
                throw new ArgumentException("Book ID must be a positive number.", nameof(bookId));

            UserId = userId;
            BookId = bookId;
            AddedDate = DateTime.UtcNow;
        }
    }
} 