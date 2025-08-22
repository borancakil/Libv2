// LibraryApp.Domain/Entities/Author.cs
namespace LibraryApp.Domain.Entities
{
    /// <summary>
    /// Represents an author in the library system
    /// </summary>
    public class Author
    {
        // Parameterless constructor for EF Core
        private Author() { }

        public int AuthorId { get; set; }

        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Author name cannot be null or empty.", nameof(value));
                _name = value.Trim();
            }
        }

        public string? Biography { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? Nationality { get; set; }

        // Photo/Image properties
        public byte[]? ProfileImage { get; set; }
        public string? ImageContentType { get; set; } // image/jpeg, image/png, etc.
        public string? ImageFileName { get; set; }

        // Navigation properties
        public ICollection<Book> Books { get; set; } = new List<Book>();

        // Public constructor
        public Author(string name) : this()
        {
            Name = name;
        }

        public Author(string name, string? biography, DateTime? birthDate, string? nationality) : this(name)
        {
            Biography = biography;
            BirthDate = birthDate;
            Nationality = nationality;
        }

        // Domain methods
        public int GetBookCount()
        {
            return Books?.Count ?? 0;
        }

        public IEnumerable<Book> GetAvailableBooks()
        {
            return Books?.Where(b => b.IsAvailable) ?? Enumerable.Empty<Book>();
        }

        /// <summary>
        /// Checks if the author has a profile image
        /// </summary>
        /// <returns>True if author has a profile image</returns>
        public bool HasProfileImage()
        {
            return ProfileImage != null && ProfileImage.Length > 0;
        }

        /// <summary>
        /// Sets the profile image for the author
        /// </summary>
        /// <param name="imageData">Image byte array</param>
        /// <param name="contentType">Image content type (e.g., image/jpeg)</param>
        /// <param name="fileName">Original file name</param>
        public void SetProfileImage(byte[] imageData, string contentType, string fileName)
        {
            ProfileImage = imageData;
            ImageContentType = contentType;
            ImageFileName = fileName;
        }

        /// <summary>
        /// Removes the profile image from the author
        /// </summary>
        public void RemoveProfileImage()
        {
            ProfileImage = null;
            ImageContentType = null;
            ImageFileName = null;
        }
    }
}