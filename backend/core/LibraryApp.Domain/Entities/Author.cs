// LibraryApp.Domain/Entities/Author.cs
namespace LibraryApp.Domain.Entities
{

    /// <summary>
    /// Represents the author of the book.
    /// </summary>
    /// 
    public class Author
    {
        public int AuthorId { get; set; }
        public string Name { get; set; }
        public ICollection<Book> Books { get; set; } = new List<Book>();
    }
}