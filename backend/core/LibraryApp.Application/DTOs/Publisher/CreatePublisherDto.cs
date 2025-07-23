namespace LibraryApp.Application.DTOs.Publisher
{
    public class CreatePublisherDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Website { get; set; }
        public string? ContactEmail { get; set; }
    }
} 