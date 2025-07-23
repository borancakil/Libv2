namespace LibraryApp.Application.DTOs.Publisher
{
    public class PublisherDto
    {
        public int PublisherId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Website { get; set; }
        public string? ContactEmail { get; set; }
        public int BookCount { get; set; }
        public int AvailableBookCount { get; set; }
    }
} 