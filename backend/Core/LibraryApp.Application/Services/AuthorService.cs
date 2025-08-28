using LibraryApp.Application.DTOs.Author;
using AutoMapper;
using LibraryApp.Application.DTOs.Book;
using LibraryApp.Application.Exceptions;
using LibraryApp.Domain.Entities;
using LibraryApp.Domain.Interfaces;
using LibraryApp.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace LibraryApp.Application.Services
{
    public class AuthorService : IAuthorService
    {
        private readonly IAuthorRepository _authorRepository;
        private readonly IBookRepository _bookRepository;
        private readonly ILoggingService _loggingService;
        private readonly IMapper _mapper;

        public AuthorService(IAuthorRepository authorRepository, IBookRepository bookRepository, ILoggingService loggingService, IMapper mapper)
        {
            _authorRepository = authorRepository ?? throw new ArgumentNullException(nameof(authorRepository));
            _bookRepository = bookRepository ?? throw new ArgumentNullException(nameof(bookRepository));
            _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<IEnumerable<AuthorDto>> GetAllAuthorsAsync(string? filter = null)
        {
            var authors = await _authorRepository.GetAllAsync(includeNavigationProperties: true) ?? new List<Author>();

            if (!string.IsNullOrWhiteSpace(filter))
            {
                var term = filter.Trim().ToLower(); // K���k harfe �evirerek kar��la�t�rma yap�yoruz.

                authors = authors.Where(a =>
                    (a.Name?.Contains(term, StringComparison.CurrentCultureIgnoreCase) == true) ||
                    (a.Books != null && a.Books.Any(b => b.Title.Contains(term, StringComparison.CurrentCultureIgnoreCase))) ||
                    (a.Biography?.Contains(term, StringComparison.CurrentCultureIgnoreCase) == true) ||
                    (a.BirthDate.HasValue && a.BirthDate.Value.ToString("yyyy").Contains(term)) 
                );

            }

            return _mapper.Map<IEnumerable<AuthorDto>>(authors);
        }


        public async Task<AuthorDto> GetAuthorByIdAsync(int id)
        {
            var author = await _authorRepository.GetByIdAsync(id, includeNavigationProperties: true);
            
            if (author == null)
            {
                throw new AuthorNotFoundException(id);
            }

            return _mapper.Map<AuthorDto>(author);
        }

        public async Task<AuthorDto> CreateAuthorAsync(CreateAuthorDto createAuthorDto)
        {
            var author = new Author(createAuthorDto.Name)
            {
                Biography = createAuthorDto.Biography,
                Nationality = createAuthorDto.Nationality
            };

            // Process profile image if provided
            if (createAuthorDto.ProfileImage != null)
            {
                await ProcessProfileImageAsync(author, createAuthorDto.ProfileImage);
            }

            await _authorRepository.AddAsync(author);
            await _authorRepository.SaveChangesAsync();

            // Business logic logging
            _loggingService.LogUserAction(0, "CREATE_AUTHOR", $"Author created: {author.Name}", new { 
                Name = author.Name,
                Nationality = author.Nationality,
                HasProfileImage = createAuthorDto.ProfileImage != null 
            });

            return _mapper.Map<AuthorDto>(author);
        }

        public async Task UpdateAuthorAsync(int id, UpdateAuthorDto updateAuthorDto)
        {
            var author = await _authorRepository.GetByIdAsync(id);
            
            if (author == null)
            {
                throw new AuthorNotFoundException(id);
            }

            // Update author properties
            author.Name = updateAuthorDto.Name;
            author.Biography = updateAuthorDto.Biography;
            author.Nationality = updateAuthorDto.Nationality;

            // Process profile image if provided
            if (updateAuthorDto.ProfileImage != null)
            {
                await ProcessProfileImageAsync(author, updateAuthorDto.ProfileImage);
            }

            _authorRepository.Update(author);
            await _authorRepository.SaveChangesAsync();

            // Business logic logging
            _loggingService.LogUserAction(0, "UPDATE_AUTHOR", $"Author updated: {author.Name}", new { 
                AuthorId = id,
                Name = author.Name,
                Nationality = author.Nationality,
                HasProfileImage = updateAuthorDto.ProfileImage != null 
            });
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
                throw new InvalidOperationException($"Cannot delete author '{author.Name}' because they have {author.Books.Count()} books.");
            }

            _authorRepository.Delete(author);
            await _authorRepository.SaveChangesAsync();

            // Business logic logging
            _loggingService.LogUserAction(0, "DELETE_AUTHOR", $"Author deleted: {author.Name}", new { 
                AuthorId = id,
                Name = author.Name 
            });
        }

        public async Task<bool> AuthorExistsAsync(int id)
        {
            return await _authorRepository.ExistsAsync(id);
        }

        public async Task<(byte[] ImageBytes, string ContentType, string FileName)?> GetProfileImageAsync(int id)
        {
            var author = await _authorRepository.GetByIdAsync(id);
            
            if (author == null)
            {
                throw new AuthorNotFoundException(id);
            }

            if (!author.HasProfileImage())
            {
                return null;
            }

            return (author.ProfileImage!, author.ImageContentType!, author.ImageFileName!);
        }

        public async Task RemoveProfileImageAsync(int id)
        {
            var author = await _authorRepository.GetByIdAsync(id);
            
            if (author == null)
            {
                throw new AuthorNotFoundException(id);
            }

            author.RemoveProfileImage();
            _authorRepository.Update(author);
            await _authorRepository.SaveChangesAsync();
        }

        public async Task<int> GetAuthorBookCountAsync(int id)
        {
            // Check if author exists first
            var authorExists = await _authorRepository.ExistsAsync(id);
            if (!authorExists)
            {
                throw new AuthorNotFoundException(id);
            }

            // Get book count directly from database without loading the collection
            return await _bookRepository.GetBookCountByAuthorAsync(id);
        }





        /// <summary>
        /// Processes and sets the profile image for an author
        /// </summary>
        /// <param name="author">The author to set the profile image for</param>
        /// <param name="imageFile">The image file to process</param>
        private async Task ProcessProfileImageAsync(Author author, IFormFile imageFile)
        {
            // Validate file
            if (imageFile == null || imageFile.Length == 0)
            {
                throw new ValidationException("ProfileImage", "Image file is required");
            }

            // Validate file size (max 5MB)
            if (imageFile.Length > 5 * 1024 * 1024)
            {
                throw new ValidationException("ProfileImage", "Image file size must be less than 5MB");
            }

            // Validate file type
            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
            if (!allowedTypes.Contains(imageFile.ContentType.ToLower()))
            {
                throw new ValidationException("ProfileImage", "Only JPEG, PNG, and GIF images are allowed");
            }

            // Read file into byte array
            using var memoryStream = new MemoryStream();
            await imageFile.CopyToAsync(memoryStream);
            var imageBytes = memoryStream.ToArray();

            // Set the profile image
            author.SetProfileImage(imageBytes, imageFile.ContentType, imageFile.FileName);
        }

        // Interface'de tanımlanan eksik metodlar
        public async Task<AuthorDto> AddAuthorAsync(CreateAuthorDto createAuthorDto)
        {
            return await CreateAuthorAsync(createAuthorDto);
        }

        public async Task<AuthorDto?> GetByIdAsync(int id)
        {
            return await GetAuthorByIdAsync(id);
        }

        public async Task<IEnumerable<BookDto>> GetAuthorBooksAsync(int authorId)
        {
            var books = await _bookRepository.GetAllAsync(includeNavigationProperties: true);
            var authorBooks = books.Where(b => b.AuthorId == authorId);

            var dtos = _mapper.Map<IEnumerable<BookDto>>(authorBooks);
            foreach (var dto in dtos)
                dto.BorrowCount = 0; // computed elsewhere when needed
            return dtos;
        }

        public async Task<(byte[]? content, string contentType, string fileName)> GetAuthorProfileImageAsync(int id)
        {
            var result = await GetProfileImageAsync(id);
            if (result == null)
                return (null, "", "");

            return (result.Value.ImageBytes, result.Value.ContentType, result.Value.FileName);
        }

        public async Task DeleteAuthorProfileImageAsync(int id)
        {
            await RemoveProfileImageAsync(id);
        }
    }
} 