using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibraryApp.Application.DTOs.Book;
using LibraryApp.Application.Exceptions;
using LibraryApp.Application.Interfaces;
using LibraryApp.Domain.Entities;
using LibraryApp.Domain.Interfaces;

namespace LibraryApp.Application.Services
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILoanRepository _loanRepository;
        private readonly IAuthorRepository _authorRepository;
        private readonly IPublisherRepository _publisherRepository;

        public BookService(IBookRepository bookRepository, IUserRepository userRepository, ILoanRepository loanRepository, IAuthorRepository authorRepository, IPublisherRepository publisherRepository)
        {
            _bookRepository = bookRepository ?? throw new ArgumentNullException(nameof(bookRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _loanRepository = loanRepository ?? throw new ArgumentNullException(nameof(loanRepository));
            _authorRepository = authorRepository ?? throw new ArgumentNullException(nameof(authorRepository));
            _publisherRepository = publisherRepository ?? throw new ArgumentNullException(nameof(publisherRepository));
        }

        public async Task<int> AddBookAsync(CreateBookDto dto)
        {
            Console.WriteLine("📚 BOOK SERVICE DEBUG - Step 1: AddBookAsync method entered");
            
            // Validation
            if (dto == null)
            {
                Console.WriteLine("📚 BOOK SERVICE DEBUG - Step 2: DTO is null - throwing ArgumentNullException");
                throw new ArgumentNullException(nameof(dto));
            }

            Console.WriteLine($"📚 BOOK SERVICE DEBUG - Step 3: DTO validation passed - Title: {dto.Title}, Year: {dto.PublicationYear}, AuthorId: {dto.AuthorId}, PublisherId: {dto.PublisherId}");

            // Validate publication year using domain logic
            if (!Book.IsValidPublicationYear(dto.PublicationYear))
            {
                Console.WriteLine($"📚 BOOK SERVICE DEBUG - Step 4: Invalid publication year: {dto.PublicationYear}");
                throw new ValidationException("PublicationYear", $"Publication year {dto.PublicationYear} is not valid");
            }

            Console.WriteLine("📚 BOOK SERVICE DEBUG - Step 5: Publication year validation passed");

            // Validate Author exists
            Console.WriteLine($"📚 BOOK SERVICE DEBUG - Step 6: Checking if Author {dto.AuthorId} exists");
            
            try
            {
                var authorExists = await _authorRepository.ExistsAsync(dto.AuthorId);
                Console.WriteLine($"📚 BOOK SERVICE DEBUG - Step 7: Author exists check result: {authorExists}");
                
                if (!authorExists)
                {
                    Console.WriteLine($"📚 BOOK SERVICE DEBUG - Step 8: Author {dto.AuthorId} does not exist - throwing ValidationException");
                    throw new ValidationException("AuthorId", $"Author with ID {dto.AuthorId} does not exist");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"📚 BOOK SERVICE DEBUG - EXCEPTION in Author validation: {ex.Message}");
                Console.WriteLine($"📚 BOOK SERVICE DEBUG - EXCEPTION Type: {ex.GetType().Name}");
                Console.WriteLine($"📚 BOOK SERVICE DEBUG - EXCEPTION StackTrace: {ex.StackTrace}");
                throw;
            }

            Console.WriteLine("📚 BOOK SERVICE DEBUG - Step 9: Author validation passed");

            // Validate Publisher exists
            Console.WriteLine($"📚 BOOK SERVICE DEBUG - Step 10: Checking if Publisher {dto.PublisherId} exists");
            
            try
            {
                var publisherExists = await _publisherRepository.ExistsAsync(dto.PublisherId);
                Console.WriteLine($"📚 BOOK SERVICE DEBUG - Step 11: Publisher exists check result: {publisherExists}");
                
                if (!publisherExists)
                {
                    Console.WriteLine($"📚 BOOK SERVICE DEBUG - Step 12: Publisher {dto.PublisherId} does not exist - throwing ValidationException");
                    throw new ValidationException("PublisherId", $"Publisher with ID {dto.PublisherId} does not exist");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"📚 BOOK SERVICE DEBUG - EXCEPTION in Publisher validation: {ex.Message}");
                Console.WriteLine($"📚 BOOK SERVICE DEBUG - EXCEPTION Type: {ex.GetType().Name}");
                Console.WriteLine($"📚 BOOK SERVICE DEBUG - EXCEPTION StackTrace: {ex.StackTrace}");
                throw;
            }

            Console.WriteLine("📚 BOOK SERVICE DEBUG - Step 13: Publisher validation passed");

            // Create book entity
            Console.WriteLine("📚 BOOK SERVICE DEBUG - Step 14: Creating Book entity");
            
            try
            {
                var book = new Book(dto.Title, dto.PublicationYear, dto.AuthorId, dto.PublisherId);
                Console.WriteLine($"📚 BOOK SERVICE DEBUG - Step 15: Book entity created with BookId: {book.BookId}");

                // Save to repository
                Console.WriteLine("📚 BOOK SERVICE DEBUG - Step 16: Adding book to repository");
                await _bookRepository.AddAsync(book);
                
                Console.WriteLine("📚 BOOK SERVICE DEBUG - Step 17: Calling SaveChangesAsync");
                await _bookRepository.SaveChangesAsync(); // CRITICAL FIX: Added SaveChangesAsync
                
                Console.WriteLine($"📚 BOOK SERVICE DEBUG - Step 18: Book saved successfully with final BookId: {book.BookId}");

                return book.BookId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"📚 BOOK SERVICE DEBUG - EXCEPTION in Book creation/save: {ex.Message}");
                Console.WriteLine($"📚 BOOK SERVICE DEBUG - EXCEPTION Type: {ex.GetType().Name}");
                Console.WriteLine($"📚 BOOK SERVICE DEBUG - EXCEPTION StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<BookDto?> GetByIdAsync(int id)
        {
            if (id <= 0)
                return null;

            var book = await _bookRepository.GetByIdAsync(id, includeNavigationProperties: true);
            if (book == null) 
                return null;

            return new BookDto
            {
                BookId = book.BookId,
                Title = book.Title,
                AuthorId = book.AuthorId,
                AuthorName = book.Author?.Name, // Navigation property access
                PublisherId = book.PublisherId,
                PublisherName = book.Publisher?.Name, // Navigation property access
                PublicationYear = book.PublicationYear,
                IsAvailable = book.IsAvailable,
                BorrowCount = book.BorrowedBooks?.Count ?? 0
            };
        }

        public async Task<IEnumerable<BookDto>> GetAllBooksAsync(bool includeUnavailable = true)
        {
            var books = await _bookRepository.GetAllAsync(includeNavigationProperties: true);

            if (!includeUnavailable)
            {
                books = books.Where(b => b.IsAvailable);
            }

            return books.Select(book => new BookDto
            {
                BookId = book.BookId,
                Title = book.Title,
                AuthorId = book.AuthorId,
                AuthorName = book.Author?.Name,
                PublisherId = book.PublisherId,
                PublisherName = book.Publisher?.Name,
                PublicationYear = book.PublicationYear,
                IsAvailable = book.IsAvailable,
                BorrowCount = book.BorrowedBooks?.Count ?? 0
            });
        }

        public async Task UpdateBookAsync(int id, UpdateBookDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (id <= 0)
                throw new ArgumentException("Book ID must be greater than zero.", nameof(id));

            var book = await _bookRepository.GetByIdAsync(id);
            if (book == null)
                throw new BookNotFoundException(id);

            if (!Book.IsValidPublicationYear(dto.PublicationYear))
                throw new ValidationException("PublicationYear", $"Publication year {dto.PublicationYear} is not valid");

            // Validate Author exists
            if (!await _authorRepository.ExistsAsync(dto.AuthorId))
                throw new ValidationException("AuthorId", $"Author with ID {dto.AuthorId} does not exist");

            // Validate Publisher exists
            if (!await _publisherRepository.ExistsAsync(dto.PublisherId))
                throw new ValidationException("PublisherId", $"Publisher with ID {dto.PublisherId} does not exist");

            // Update book properties
            book.Title = dto.Title;
            book.PublicationYear = dto.PublicationYear;
            book.AuthorId = dto.AuthorId;
            book.PublisherId = dto.PublisherId;
            book.SetAvailability(dto.IsAvailable); // Use domain method instead of direct property access

            _bookRepository.Update(book);
            await _bookRepository.SaveChangesAsync(); // CRITICAL: This was missing!
        }

        public async Task BorrowBookAsync(BorrowBookDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            // Custom validation
            if (!dto.IsValid(out string validationError))
                throw new ValidationException("BorrowData", validationError);

            // TRANSACTION MANAGEMENT - Get entities first
            var book = await _bookRepository.GetByIdAsync(dto.BookId);
            var user = await _userRepository.GetByIdAsync(dto.UserId);

            if (book == null)
                throw new BookNotFoundException(dto.BookId);

            if (user == null)
                throw new KeyNotFoundException($"User with ID {dto.UserId} not found.");

            // Check if book is available BEFORE trying to borrow
            if (!book.IsAvailable)
                throw new BookNotAvailableException(book.BookId, book.Title);

            try
            {
                // Use domain logic for borrowing
                book.MarkAsBorrowed();
                _bookRepository.Update(book);

                // Create loan record using domain constructor
                var loan = new Loan(dto.BookId, dto.UserId, dto.BorrowDate, dto.DueDate);

                await _loanRepository.AddAsync(loan);
                
                // CRITICAL FIX: Save all changes in one transaction
                await _bookRepository.SaveChangesAsync();
            }
            catch (InvalidOperationException ex)
            {
                // Domain logic threw an exception
                throw new BookNotAvailableException(book.BookId, book.Title, ex.Message);
            }
        }

        public async Task DeleteBookAsync(int id)
        {
            var book = await _bookRepository.GetByIdAsync(id);

            if (book == null)
                throw new BookNotFoundException(id);

            // Use domain logic to check if book can be deleted
            if (!book.CanBeDeleted())
                throw new InvalidOperationException($"Cannot delete book '{book.Title}' because it is currently on loan.");

            _bookRepository.Delete(book);
            await _bookRepository.SaveChangesAsync(); // Ensure consistency
        }

        public async Task ReturnBookAsync(int bookId, int userId)
        {
            var book = await _bookRepository.GetByIdAsync(bookId);
            if (book == null)
                throw new BookNotFoundException(bookId);

            // Find active loan
            var activeLoan = await _loanRepository.GetActiveLoanAsync(bookId, userId);
            if (activeLoan == null)
                throw new InvalidOperationException($"No active loan found for book {bookId} and user {userId}");

            // Use domain logic
            book.MarkAsReturned();
            activeLoan.MarkAsReturned();

            _bookRepository.Update(book);
            _loanRepository.Update(activeLoan);
            
            await _bookRepository.SaveChangesAsync();
        }

        public async Task<bool> BookExistsAsync(int id)
        {
            return await _bookRepository.ExistsAsync(id);
        }
    }
}