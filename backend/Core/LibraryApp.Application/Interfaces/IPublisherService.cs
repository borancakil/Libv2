using LibraryApp.Application.DTOs.Publisher;
using LibraryApp.Application.DTOs.Book;

namespace LibraryApp.Application.Interfaces
{
    public interface IPublisherService
    {
        Task<IEnumerable<PublisherDto>> GetAllPublishersAsync(string? filter);
        
        Task<IEnumerable<PublisherListDto>> GetAllPublishersForListAsync(string? filter = null);
        
        Task<PublisherDto> GetPublisherByIdAsync(int id);
        
        Task<PublisherDto> CreatePublisherAsync(CreatePublisherDto createPublisherDto);
        
        Task UpdatePublisherAsync(int id, UpdatePublisherDto updatePublisherDto);
        
        Task DeletePublisherAsync(int id);
        
        Task<bool> PublisherExistsAsync(int id);

        Task<(byte[] ImageBytes, string ContentType, string FileName)?> GetLogoImageAsync(int id);

        Task RemoveLogoImageAsync(int id);

        Task<int> GetPublisherBookCountAsync(int id);

        // Controller'da kullanılan eksik metodlar
        Task<PublisherDto> AddPublisherAsync(CreatePublisherDto createPublisherDto);
        Task<PublisherDto?> GetByIdAsync(int id);
        Task<IEnumerable<BookDto>> GetPublisherBooksAsync(int publisherId);
        Task<(byte[]? content, string contentType, string fileName)> GetPublisherLogoImageAsync(int id);
        Task DeletePublisherLogoImageAsync(int id);
    }
} 