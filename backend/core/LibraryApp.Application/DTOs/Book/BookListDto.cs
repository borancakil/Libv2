namespace LibraryApp.Application.DTOs.Book
{
    public class BookListDto
    {
        public int BookId { get; set; }
        public string Title { get; set; }
        public int PublicationYear { get; set; }
        public bool IsAvailable { get; set; }
        public string? CoverImageUrl { get; set; } // We will construct this URL

        public int AuthorId { get; set; }
        public string AuthorName { get; set; }

        public int PublisherId { get; set; }
        public string PublisherName { get; set; }
    }
}