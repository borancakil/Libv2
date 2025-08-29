
using LibraryApp.Application.DTOs.Author;
using LibraryApp.Application.DTOs.Book;

namespace LibraryApp.Application.Interfaces
{
    public interface IAuthorService
    {
        Task<IEnumerable<AuthorDto>> GetAllAuthorsAsync(string? filter);
        
        Task<IEnumerable<AuthorListDto>> GetAllAuthorsForListAsync(string? filter = null);

        Task<AuthorDto> GetAuthorByIdAsync(int id);

        Task<AuthorDto> CreateAuthorAsync(CreateAuthorDto createAuthorDto);

        Task UpdateAuthorAsync(int id, UpdateAuthorDto updateAuthorDto);

        Task DeleteAuthorAsync(int id);

        Task<bool> AuthorExistsAsync(int id);

        Task<(byte[] ImageBytes, string ContentType, string FileName)?> GetProfileImageAsync(int id);

        Task RemoveProfileImageAsync(int id);

        Task<int> GetAuthorBookCountAsync(int id);

        // Controller'da kullanılan eksik metodlar
        Task<AuthorDto> AddAuthorAsync(CreateAuthorDto createAuthorDto);
        Task<AuthorDto?> GetByIdAsync(int id);
        Task<IEnumerable<BookDto>> GetAuthorBooksAsync(int authorId);
        Task<(byte[]? content, string contentType, string fileName)> GetAuthorProfileImageAsync(int id);
        Task DeleteAuthorProfileImageAsync(int id);
    }
}