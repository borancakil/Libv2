namespace LibraryApp.Application.DTOs.Author
{
    public class AuthorDto
    {
        public int AuthorId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Biography { get; set; }
        public string? Nationality { get; set; }
        public int BookCount { get; set; }
        public int AvailableBookCount { get; set; }
    }
} 