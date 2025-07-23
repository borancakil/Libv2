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
    }
}