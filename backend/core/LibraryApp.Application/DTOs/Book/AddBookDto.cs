namespace LibraryApp.Application.DTOs.Book
{
    /// <summary>
    /// DTO for creating a new book
    /// Only FluentValidation is used for validation
    /// </summary>
    public class CreateBookDto
    {
        public string Title { get; set; } = string.Empty;
        public int PublicationYear { get; set; }
        public int AuthorId { get; set; }
        public int PublisherId { get; set; }
    }
}
