using LibraryApp.Application.DTOs.Author;
using LibraryApp.Application.Exceptions;
using LibraryApp.Domain.Entities;
using LibraryApp.Domain.Interfaces;

namespace LibraryApp.Application.Services
{
    public class AuthorService
    {
        private readonly IAuthorRepository _authorRepository;

        public AuthorService(IAuthorRepository authorRepository)
        {
            _authorRepository = authorRepository;
        }

        public async Task<IEnumerable<AuthorDto>> GetAllAuthorsAsync()
        {
            var authors = await _authorRepository.GetAllAsync(includeNavigationProperties: true);
            
            return authors.Select(author => new AuthorDto
            {
                AuthorId = author.AuthorId,
                Name = author.Name,
                Biography = author.Biography,
                Nationality = author.Nationality,
                BookCount = author.GetBookCount(),
                AvailableBookCount = author.GetAvailableBooks().Count()
            });
        }

        public async Task<AuthorDto> GetAuthorByIdAsync(int id)
        {
            var author = await _authorRepository.GetByIdAsync(id, includeNavigationProperties: true);
            
            if (author == null)
            {
                throw new AuthorNotFoundException(id);
            }

            return new AuthorDto
            {
                AuthorId = author.AuthorId,
                Name = author.Name,
                Biography = author.Biography,
                Nationality = author.Nationality,
                BookCount = author.GetBookCount(),
                AvailableBookCount = author.GetAvailableBooks().Count()
            };
        }

        public async Task<AuthorDto> CreateAuthorAsync(CreateAuthorDto createAuthorDto)
        {
            var author = new Author(createAuthorDto.Name)
            {
                Biography = createAuthorDto.Biography,
                Nationality = createAuthorDto.Nationality
            };

            await _authorRepository.AddAsync(author);
            await _authorRepository.SaveChangesAsync();

            return new AuthorDto
            {
                AuthorId = author.AuthorId,
                Name = author.Name,
                Biography = author.Biography,
                Nationality = author.Nationality,
                BookCount = 0,
                AvailableBookCount = 0
            };
        }

        public async Task UpdateAuthorAsync(int id, UpdateAuthorDto updateAuthorDto)
        {
            var author = await _authorRepository.GetByIdAsync(id);
            
            if (author == null)
            {
                throw new AuthorNotFoundException(id);
            }

            author.Name = updateAuthorDto.Name;
            author.Biography = updateAuthorDto.Biography;
            author.Nationality = updateAuthorDto.Nationality;

            _authorRepository.Update(author);
            await _authorRepository.SaveChangesAsync();
        }

        public async Task DeleteAuthorAsync(int id)
        {
            var author = await _authorRepository.GetByIdAsync(id, includeNavigationProperties: true);
            
            if (author == null)
            {
                throw new AuthorNotFoundException(id);
            }

            // Check if author has books
            if (author.Books != null && author.Books.Any())
            {
                throw new InvalidOperationException($"Cannot delete author '{author.Name}' because they have {author.Books.Count} book(s) associated.");
            }

            _authorRepository.Delete(author);
            await _authorRepository.SaveChangesAsync();
        }

        public async Task<bool> AuthorExistsAsync(int id)
        {
            return await _authorRepository.ExistsAsync(id);
        }
    }
} 