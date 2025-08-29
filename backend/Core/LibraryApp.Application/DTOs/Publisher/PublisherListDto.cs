namespace LibraryApp.Application.DTOs.Publisher
{
    public class PublisherListDto
    {
        public int PublisherId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? ContactEmail { get; set; }
        public DateTime? EstablishedDate { get; set; }
        public int BookCount { get; set; }
        public string? LogoImageUrl { get; set; }
    }
}
