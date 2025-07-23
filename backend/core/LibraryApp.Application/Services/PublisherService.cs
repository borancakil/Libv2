using LibraryApp.Application.DTOs.Publisher;
using LibraryApp.Application.Exceptions;
using LibraryApp.Domain.Entities;
using LibraryApp.Domain.Interfaces;

namespace LibraryApp.Application.Services
{
    public class PublisherService
    {
        private readonly IPublisherRepository _publisherRepository;

        public PublisherService(IPublisherRepository publisherRepository)
        {
            _publisherRepository = publisherRepository;
        }

        public async Task<IEnumerable<PublisherDto>> GetAllPublishersAsync()
        {
            var publishers = await _publisherRepository.GetAllAsync(includeNavigationProperties: true);
            
            return publishers.Select(publisher => new PublisherDto
            {
                PublisherId = publisher.PublisherId,
                Name = publisher.Name,
                Address = publisher.Address,
                Website = publisher.Website,
                ContactEmail = publisher.ContactEmail,
                BookCount = publisher.GetBookCount(),
                AvailableBookCount = publisher.GetAvailableBooks().Count()
            });
        }

        public async Task<PublisherDto> GetPublisherByIdAsync(int id)
        {
            var publisher = await _publisherRepository.GetByIdAsync(id, includeNavigationProperties: true);
            
            if (publisher == null)
            {
                throw new PublisherNotFoundException(id);
            }

            return new PublisherDto
            {
                PublisherId = publisher.PublisherId,
                Name = publisher.Name,
                Address = publisher.Address,
                Website = publisher.Website,
                ContactEmail = publisher.ContactEmail,
                BookCount = publisher.GetBookCount(),
                AvailableBookCount = publisher.GetAvailableBooks().Count()
            };
        }

        public async Task<PublisherDto> CreatePublisherAsync(CreatePublisherDto createPublisherDto)
        {
            var publisher = new Publisher(createPublisherDto.Name)
            {
                Address = createPublisherDto.Address,
                Website = createPublisherDto.Website,
                ContactEmail = createPublisherDto.ContactEmail
            };

            await _publisherRepository.AddAsync(publisher);
            await _publisherRepository.SaveChangesAsync();

            return new PublisherDto
            {
                PublisherId = publisher.PublisherId,
                Name = publisher.Name,
                Address = publisher.Address,
                Website = publisher.Website,
                ContactEmail = publisher.ContactEmail,
                BookCount = 0,
                AvailableBookCount = 0
            };
        }

        public async Task UpdatePublisherAsync(int id, UpdatePublisherDto updatePublisherDto)
        {
            var publisher = await _publisherRepository.GetByIdAsync(id);
            
            if (publisher == null)
            {
                throw new PublisherNotFoundException(id);
            }

            publisher.Name = updatePublisherDto.Name;
            publisher.Address = updatePublisherDto.Address;
            publisher.Website = updatePublisherDto.Website;
            publisher.ContactEmail = updatePublisherDto.ContactEmail;

            _publisherRepository.Update(publisher);
            await _publisherRepository.SaveChangesAsync();
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
                throw new InvalidOperationException($"Cannot delete publisher '{publisher.Name}' because they have {publisher.Books.Count} book(s) associated.");
            }

            _publisherRepository.Delete(publisher);
            await _publisherRepository.SaveChangesAsync();
        }

        public async Task<bool> PublisherExistsAsync(int id)
        {
            return await _publisherRepository.ExistsAsync(id);
        }
    }
} 