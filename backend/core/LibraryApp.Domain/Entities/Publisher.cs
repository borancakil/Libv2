namespace LibraryApp.Domain.Entities
{
    /// <summary>
    /// Represents the publisher of a book.
    /// </summary>
    public class Publisher
    {
        public int PublisherId { get; set; }
        public string Name { get; set; }

        /// <summary>
        /// A collection of books published by this publisher.
        /// </summary>
        public ICollection<Book> Books { get; set; } = new List<Book>();
    }
}