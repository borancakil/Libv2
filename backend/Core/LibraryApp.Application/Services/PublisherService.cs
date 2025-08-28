using LibraryApp.Application.DTOs.Publisher;
using LibraryApp.Application.DTOs.Book;
using LibraryApp.Application.Exceptions;
using LibraryApp.Application.Interfaces;
using LibraryApp.Domain.Entities;
using LibraryApp.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using AutoMapper;

namespace LibraryApp.Application.Services
{
    public class PublisherService : IPublisherService
    {
        private readonly IPublisherRepository _publisherRepository;
        private readonly IBookRepository _bookRepository;
        private readonly ILoggingService _loggingService;
        private readonly IMapper _mapper;

        public PublisherService(IPublisherRepository publisherRepository, IBookRepository bookRepository, ILoggingService loggingService, IMapper mapper)
        {
            _publisherRepository = publisherRepository ?? throw new ArgumentNullException(nameof(publisherRepository));
            _bookRepository = bookRepository ?? throw new ArgumentNullException(nameof(bookRepository));
            _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<IEnumerable<PublisherDto>> GetAllPublishersAsync(string? filter = null)
        {
            var publishers = await _publisherRepository.GetAllAsync(includeNavigationProperties: true)?? [];

            if (!string.IsNullOrWhiteSpace(filter))
            {
                var term = filter.Trim().ToLower(); 

                publishers = publishers.Where(p =>
                    (p.Address?.Contains(term, StringComparison.CurrentCultureIgnoreCase) == true) ||
                    (p.ContactEmail?.Contains(term, StringComparison.CurrentCultureIgnoreCase) == true) ||
                    (p.Name?.Contains(term, StringComparison.CurrentCultureIgnoreCase) == true)
                );
            }

            return _mapper.Map<IEnumerable<PublisherDto>>(publishers);
        }

        public async Task<PublisherDto> GetPublisherByIdAsync(int id)
        {
            var publisher = await _publisherRepository.GetByIdAsync(id, includeNavigationProperties: true);
            
            if (publisher == null)
            {
                throw new PublisherNotFoundException(id);
            }

            return _mapper.Map<PublisherDto>(publisher);
        }

        public async Task<PublisherDto> CreatePublisherAsync(CreatePublisherDto createPublisherDto)
        {
            var publisher = new Publisher(createPublisherDto.Name)
            {
                Address = createPublisherDto.Address,
                Website = createPublisherDto.Website,
                ContactEmail = createPublisherDto.ContactEmail
            };

            // Process logo image if provided
            if (createPublisherDto.LogoImage != null)
            {
                await ProcessLogoImageAsync(publisher, createPublisherDto.LogoImage);
            }

            await _publisherRepository.AddAsync(publisher);
            await _publisherRepository.SaveChangesAsync();

            // Business logic logging
            _loggingService.LogUserAction(0, "CREATE_PUBLISHER", $"Publisher created: {publisher.Name}", new { 
                Name = publisher.Name,
                Address = publisher.Address,
                ContactEmail = publisher.ContactEmail,
                HasLogoImage = createPublisherDto.LogoImage != null 
            });

            return _mapper.Map<PublisherDto>(publisher);
        }

        public async Task UpdatePublisherAsync(int id, UpdatePublisherDto updatePublisherDto)
        {
            var publisher = await _publisherRepository.GetByIdAsync(id);
            
            if (publisher == null)
            {
                throw new PublisherNotFoundException(id);
            }

            // Update publisher properties
            publisher.Name = updatePublisherDto.Name;
            publisher.Address = updatePublisherDto.Address;
            publisher.Website = updatePublisherDto.Website;
            publisher.ContactEmail = updatePublisherDto.ContactEmail;

            // Process logo image if provided
            if (updatePublisherDto.LogoImage != null)
            {
                await ProcessLogoImageAsync(publisher, updatePublisherDto.LogoImage);
            }

            _publisherRepository.Update(publisher);
            await _publisherRepository.SaveChangesAsync();

            // Business logic logging
            _loggingService.LogUserAction(0, "UPDATE_PUBLISHER", $"Publisher updated: {publisher.Name}", new { 
                PublisherId = id,
                Name = publisher.Name,
                Address = publisher.Address,
                ContactEmail = publisher.ContactEmail,
                HasLogoImage = updatePublisherDto.LogoImage != null 
            });
        }

        public async Task DeletePublisherAsync(int id)
        {
            var publisher = await _publisherRepository.GetByIdAsync(id, includeNavigationProperties: true);
            
            if (publisher == null)
            {
                throw new PublisherNotFoundException(id);
            }

            // Check if publisher has books
            if (publisher.Books != null && publisher.Books.Any())
            {
                throw new InvalidOperationException($"Cannot delete publisher '{publisher.Name}' because they have {publisher.Books.Count()} books.");
            }

            _publisherRepository.Delete(publisher);
            await _publisherRepository.SaveChangesAsync();

            // Business logic logging
            _loggingService.LogUserAction(0, "DELETE_PUBLISHER", $"Publisher deleted: {publisher.Name}", new { 
                PublisherId = id,
                Name = publisher.Name 
            });
        }

        public async Task<bool> PublisherExistsAsync(int id)
        {
            return await _publisherRepository.ExistsAsync(id);
        }

        public async Task<(byte[] ImageBytes, string ContentType, string FileName)?> GetLogoImageAsync(int id)
        {
            var publisher = await _publisherRepository.GetByIdAsync(id);
            
            if (publisher == null)
            {
                throw new PublisherNotFoundException(id);
            }

            if (!publisher.HasLogoImage())
            {
                return null;
            }

            return (publisher.LogoImage!, publisher.ImageContentType!, publisher.ImageFileName!);
        }

        public async Task RemoveLogoImageAsync(int id)
        {
            var publisher = await _publisherRepository.GetByIdAsync(id);
            
            if (publisher == null)
            {
                throw new PublisherNotFoundException(id);
            }

            publisher.RemoveLogoImage();
            _publisherRepository.Update(publisher);
            await _publisherRepository.SaveChangesAsync();
        }

        public async Task<int> GetPublisherBookCountAsync(int id)
        {
            // Check if publisher exists first
            var publisherExists = await _publisherRepository.ExistsAsync(id);
            if (!publisherExists)
            {
                throw new PublisherNotFoundException(id);
            }

            // Get book count directly from database without loading the collection
            return await _bookRepository.GetBookCountByPublisherAsync(id);
        }





        /// <summary>
        /// Processes and sets the logo image for a publisher
        /// </summary>
        /// <param name="publisher">The publisher to set the logo image for</param>
        /// <param name="imageFile">The image file to process</param>
        private async Task ProcessLogoImageAsync(Publisher publisher, IFormFile imageFile)
        {
            // Validate file
            if (imageFile == null || imageFile.Length == 0)
            {
                throw new ValidationException("LogoImage", "Image file is required");
            }

            // Validate file size (max 5MB)
            if (imageFile.Length > 5 * 1024 * 1024)
            {
                throw new ValidationException("LogoImage", "Image file size must be less than 5MB");
            }

            // Validate file type
            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
            if (!allowedTypes.Contains(imageFile.ContentType.ToLower()))
            {
                throw new ValidationException("LogoImage", "Only JPEG, PNG, and GIF images are allowed");
            }

            // Read file into byte array
            using var memoryStream = new MemoryStream();
            await imageFile.CopyToAsync(memoryStream);
            var imageBytes = memoryStream.ToArray();

            // Set the logo image
            publisher.SetLogoImage(imageBytes, imageFile.ContentType, imageFile.FileName);
        }

        // Interface'de tanımlanan eksik metodlar
        public async Task<PublisherDto> AddPublisherAsync(CreatePublisherDto createPublisherDto)
        {
            return await CreatePublisherAsync(createPublisherDto);
        }

        public async Task<PublisherDto?> GetByIdAsync(int id)
        {
            return await GetPublisherByIdAsync(id);
        }

        public async Task<IEnumerable<BookDto>> GetPublisherBooksAsync(int publisherId)
        {
            var books = await _bookRepository.GetAllAsync(includeNavigationProperties: true);
            var publisherBooks = books.Where(b => b.PublisherId == publisherId);

            var dtos = _mapper.Map<IEnumerable<BookDto>>(publisherBooks);
            foreach (var dto in dtos)
                dto.BorrowCount = 0;
            return dtos;
        }

        public async Task<(byte[]? content, string contentType, string fileName)> GetPublisherLogoImageAsync(int id)
        {
            var result = await GetLogoImageAsync(id);
            if (result == null)
                return (null, "", "");

            return (result.Value.ImageBytes, result.Value.ContentType, result.Value.FileName);
        }

        public async Task DeletePublisherLogoImageAsync(int id)
        {
            await RemoveLogoImageAsync(id);
        }
    }
} 