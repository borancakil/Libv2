namespace LibraryApp.Application.DTOs.Author
{
    public class AuthorListDto
    {
        public int AuthorId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Nationality { get; set; }
        public DateTime? BirthDate { get; set; }
        public int BookCount { get; set; }
        public string? ProfileImageUrl { get; set; }
    }
}
