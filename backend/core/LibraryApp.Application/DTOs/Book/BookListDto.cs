namespace LibraryApp.Application.DTOs.Book
{
    /// <summary>
    /// DTO representing a book for list view operations (optimized for performance)
    /// </summary>
    public sealed class BookListDto
    {
        public int BookId { get; set; }
        public string Title { get; set; } = "";
        public int? PublicationYear { get; set; }
        public string AuthorName { get; set; } = "";
        public string PublisherName { get; set; } = "";
        public string CategoryName { get; set; } = "";
        public bool IsAvailable { get; set; }         
    }
}
