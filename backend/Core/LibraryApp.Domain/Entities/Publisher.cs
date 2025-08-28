namespace LibraryApp.Domain.Entities
{
    /// <summary>
    /// Represents a publisher in the library system
    /// </summary>
    public class Publisher
    {
        // Parameterless constructor for EF Core
        private Publisher() { }

        public int PublisherId { get; set; }

        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Publisher name cannot be null or empty.", nameof(value));
                _name = value.Trim();
            }
        }

        public string? Address { get; set; }
        public string? Website { get; set; }
        public string? ContactEmail { get; set; }
        public DateTime? EstablishedDate { get; set; }

        // Photo/Image properties
        public byte[]? LogoImage { get; set; }
        public string? ImageContentType { get; set; } // image/jpeg, image/png, etc.
        public string? ImageFileName { get; set; }

        // Navigation properties
        public ICollection<Book> Books { get; set; } = new List<Book>();

        // Public constructor
        public Publisher(string name) : this()
        {
            Name = name;
        }

        public Publisher(string name, string? address, string? website, string? contactEmail) : this(name)
        {
            Address = address;
            Website = website;
            ContactEmail = contactEmail;
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

        public bool IsValidWebsite(string? website)
        {
            if (string.IsNullOrWhiteSpace(website))
                return true;

            return Uri.TryCreate(website, UriKind.Absolute, out _);
        }

        /// <summary>
        /// Checks if the publisher has a logo image
        /// </summary>
        /// <returns>True if publisher has a logo image</returns>
        public bool HasLogoImage()
        {
            return LogoImage != null && LogoImage.Length > 0;
        }

        /// <summary>
        /// Sets the logo image for the publisher
        /// </summary>
        /// <param name="imageData">Image byte array</param>
        /// <param name="contentType">Image content type (e.g., image/jpeg)</param>
        /// <param name="fileName">Original file name</param>
        public void SetLogoImage(byte[] imageData, string contentType, string fileName)
        {
            LogoImage = imageData;
            ImageContentType = contentType;
            ImageFileName = fileName;
        }

        /// <summary>
        /// Removes the logo image from the publisher
        /// </summary>
        public void RemoveLogoImage()
        {
            LogoImage = null;
            ImageContentType = null;
            ImageFileName = null;
        }
    }
}