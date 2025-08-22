

namespace LibraryApp.Domain.Entities
{
    /// <summary>
    /// Represents a single book in the library's catalog.
    /// This is a root entity in our domain.
    /// </summary>
    public class Book
    {
        // Parameterless constructor for EF Core
        private Book() 
        {
            IsAvailable = true; 
        }

        public int BookId { get; set; }
        
        private string _title = string.Empty;
        public string Title 
        { 
            get => _title;
            set 
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Title cannot be null or empty.", nameof(value));
                _title = value;
            }
        }
        
        public int PublicationYear { get; set; }
        public bool IsAvailable { get; private set; } = true;

        // Rating property
        private decimal _rating = 0;
        public decimal Rating 
        { 
            get => _rating;
            set 
            {
                if (value < 0 || value > 5)
                    throw new ArgumentException("Rating must be between 0 and 5.", nameof(value));
                _rating = value;
            }
        }

        // Category relationship
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        // Photo/Image properties
        public byte[]? CoverImage { get; set; }
        public string? ImageContentType { get; set; } // image/jpeg, image/png, etc.
        public string? ImageFileName { get; set; }

        public int AuthorId { get; set; }
        public Author? Author { get; set; } 

        public int PublisherId { get; set; }
        public Publisher? Publisher { get; set; }

        public ICollection<Loan> BorrowedBooks { get; set; } = new List<Loan>();
        public ICollection<UserFavoriteBook> FavoritedByUsers { get; set; } = new List<UserFavoriteBook>();

        public Book(string title)
        {
            Title = title; 
        }

        public Book(string title, int publicationYear, int authorId, int publisherId, int categoryId) : this(title)
        {
            PublicationYear = publicationYear;
            AuthorId = authorId;
            PublisherId = publisherId;
            CategoryId = categoryId;
            IsAvailable = true;
            Rating = 0;
        }

        public Book(string title, int publicationYear, int authorId, int publisherId, int categoryId, decimal rating) : this(title)
        {
            PublicationYear = publicationYear;
            AuthorId = authorId;
            PublisherId = publisherId;
            CategoryId = categoryId;
            IsAvailable = true;
            Rating = rating;
        }

        public void MarkAsBorrowed()
        {
            if (!IsAvailable)
            {
                throw new InvalidOperationException("Book is not available to be borrowed.");
            }
            IsAvailable = false;
        }

        public void MarkAsReturned()
        {
            IsAvailable = true;
        }

        public void SetAvailability(bool isAvailable)
        {
            IsAvailable = isAvailable;
        }

        public bool CanBeDeleted()
        {
            return IsAvailable;
        }

        public static bool IsValidPublicationYear(int year)
        {
            return year >= 1000 && year <= DateTime.Now.Year + 1;
        }

        /// <summary>
        /// Gets the number of users who have this book in their favorites
        /// </summary>
        /// <returns>Number of users who favorited this book</returns>
        public int GetFavoriteCount()
        {
            return FavoritedByUsers?.Count ?? 0;
        }

        /// <summary>
        /// Checks if a specific user has this book in their favorites
        /// </summary>
        /// <param name="userId">User ID to check</param>
        /// <returns>True if user has this book in favorites</returns>
        public bool IsFavoritedByUser(int userId)
        {
            return FavoritedByUsers?.Any(ufb => ufb.UserId == userId) ?? false;
        }

        /// <summary>
        /// Checks if the book has a cover image
        /// </summary>
        /// <returns>True if book has a cover image</returns>
        public bool HasCoverImage()
        {
            // If CoverImage is not loaded (null), check metadata
            if (CoverImage == null)
            {
                return !string.IsNullOrEmpty(ImageFileName) && !string.IsNullOrEmpty(ImageContentType);
            }
            
            return CoverImage.Length > 0;
        }

        /// <summary>
        /// Sets the cover image for the book
        /// </summary>
        /// <param name="imageData">Image byte array</param>
        /// <param name="contentType">Image content type (e.g., image/jpeg)</param>
        /// <param name="fileName">Original file name</param>
        public void SetCoverImage(byte[] imageData, string contentType, string fileName)
        {
            CoverImage = imageData;
            ImageContentType = contentType;
            ImageFileName = fileName;
        }

        /// <summary>
        /// Removes the cover image from the book
        /// </summary>
        public void RemoveCoverImage()
        {
            CoverImage = null;
            ImageContentType = null;
            ImageFileName = null;
        }

        /// <summary>
        /// Updates the rating of the book
        /// </summary>
        /// <param name="newRating">New rating value (0-5)</param>
        public void UpdateRating(decimal newRating)
        {
            Rating = newRating;
        }

        /// <summary>
        /// Updates the category of the book
        /// </summary>
        /// <param name="categoryId">New category ID</param>
        public void UpdateCategory(int categoryId)
        {
            CategoryId = categoryId;
        }

        /// <summary>
        /// Gets the category name as string
        /// </summary>
        /// <returns>Category name</returns>
        public string GetCategoryName()
        {
            return Category?.Name ?? "Unknown";
        }

        /// <summary>
        /// Checks if the book has a rating
        /// </summary>
        /// <returns>True if book has a rating greater than 0</returns>
        public bool HasRating()
        {
            return Rating > 0;
        }
    }
}