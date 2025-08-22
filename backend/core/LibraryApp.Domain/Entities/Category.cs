namespace LibraryApp.Domain.Entities
{
    /// <summary>
    /// Represents a book category in the library system.
    /// </summary>
    public class Category
    {
        private Category() { }

        public int CategoryId { get; set; }
        
        private string _name = string.Empty;
        public string Name 
        { 
            get => _name;
            set 
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Category name cannot be null or empty.", nameof(value));
                _name = value;
            }
        }

        private string? _description;
        public string? Description 
        { 
            get => _description;
            set => _description = value;
        }

        // Navigation property
        public ICollection<Book> Books { get; set; } = new List<Book>();

        public Category(string name)
        {
            Name = name;
        }

        public Category(string name, string? description) : this(name)
        {
            Description = description;
        }

        /// <summary>
        /// Gets the number of books in this category
        /// </summary>
        /// <returns>Number of books in this category</returns>
        public int GetBookCount()
        {
            return Books?.Count ?? 0;
        }

        /// <summary>
        /// Checks if this category has any books
        /// </summary>
        /// <returns>True if category has books</returns>
        public bool HasBooks()
        {
            return GetBookCount() > 0;
        }
    }
} 