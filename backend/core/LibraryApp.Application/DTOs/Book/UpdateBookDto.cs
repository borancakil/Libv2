namespace LibraryApp.Application.DTOs.Book
{
    /// <summary>
    /// DTO for updating an existing book
    /// Only FluentValidation is used for validation
    /// </summary>
    public class UpdateBookDto
    {
        public string Title { get; set; } = string.Empty;
        public int PublicationYear { get; set; }
        public bool IsAvailable { get; set; }
        public int AuthorId { get; set; }
        public int PublisherId { get; set; }
    }
}
