using AutoMapper;
using LibraryApp.Application.DTOs.Book;
using LibraryApp.Application.DTOs.User;
using LibraryApp.Application.Exceptions;
using LibraryApp.Application.Interfaces;
using LibraryApp.Domain.Entities;
using LibraryApp.Domain.Interfaces;
using Microsoft.AspNetCore.Http;


using Microsoft.EntityFrameworkCore;

namespace LibraryApp.Application.Services
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILoanRepository _loanRepository;
        private readonly IAuthorRepository _authorRepository;
        private readonly IPublisherRepository _publisherRepository;
        private readonly ILoggingService _loggingService;
        private readonly IMapper _mapper;   

        public BookService(IBookRepository bookRepository, IUserRepository userRepository, ILoanRepository loanRepository, IAuthorRepository authorRepository, IPublisherRepository publisherRepository, ILoggingService loggingService, IMapper mapper)
        {
            _bookRepository = bookRepository ?? throw new ArgumentNullException(nameof(bookRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _loanRepository = loanRepository ?? throw new ArgumentNullException(nameof(loanRepository));
            _authorRepository = authorRepository ?? throw new ArgumentNullException(nameof(authorRepository));
            _publisherRepository = publisherRepository ?? throw new ArgumentNullException(nameof(publisherRepository));
            _loggingService= loggingService?? throw new ArgumentNullException(nameof(loggingService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

        }


        public async Task<int> AddBookAsync(CreateBookDto dto)
        {
            // Validation
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            // Validate publication year using domain logic
            if (!Book.IsValidPublicationYear(dto.PublicationYear))
            {
                throw new ValidationException("PublicationYear", $"Publication year {dto.PublicationYear} is not valid");
            }

            // Validate author exists
            var authorExists = await _authorRepository.ExistsAsync(dto.AuthorId);
            if (!authorExists)
            {
                throw new ValidationException("AuthorId", $"Author with ID {dto.AuthorId} does not exist");
            }

            // Validate publisher exists
            var publisherExists = await _publisherRepository.ExistsAsync(dto.PublisherId);
            if (!publisherExists)
            {
                throw new ValidationException("PublisherId", $"Publisher with ID {dto.PublisherId} does not exist");
            }

            var book = _mapper.Map<Book>(dto);

            // Handle cover image if provided
            if (dto.CoverImage != null)
            {
                await ProcessCoverImageAsync(book, dto.CoverImage);
            }

            await _bookRepository.AddAsync(book);
            await _bookRepository.SaveChangesAsync();

            // Log CRUD operation
            _loggingService.LogCrudOperation("CREATE", "Book", book.BookId, 0, new { 
                Title = dto.Title,
                AuthorId = dto.AuthorId, 
                PublisherId = dto.PublisherId,
                PublicationYear = dto.PublicationYear,
                HasCoverImage = dto.CoverImage != null 
            });


            return book.BookId;
        }

        public async Task<BookDto?> GetByIdAsync(int id)
        {
            if (id <= 0)
                return null;
    
            // Optimized loading for detail view
            var book = await _bookRepository.GetByIdForDetailAsync(id);
            if (book == null) 
                return null;

            // Get borrowed books count separately to avoid loading the entire collection
            var borrowCount = await _bookRepository.GetBorrowedBooksCountAsync(id);

            var dto = _mapper.Map<BookDto>(book);

            dto.BorrowCount = borrowCount;

            return dto;

        }
        public async Task<IEnumerable<BookDto>> GetAllBooksAsync(string? filter = null, bool includeUnavailable = true)
        {
            var books = _bookRepository.GetAllForList();

            if (!string.IsNullOrWhiteSpace(filter))
            {
                var term = filter.Trim();
                var like = $"%{term}%";

                bool parsedYear = int.TryParse(term, out var year);

                books = books.Where(b =>
                    EF.Functions.Like(b.Title ?? "", like) ||
                    (b.Author != null && EF.Functions.Like(b.Author.Name ?? "", like)) ||
                    (b.Publisher != null && EF.Functions.Like(b.Publisher.Name ?? "", like)) ||
                    (b.Category != null && EF.Functions.Like(b.Category.Name ?? "", like)) ||
                    (parsedYear && b.PublicationYear == year)
                );
            }


            if (!includeUnavailable)
                books = books.Where(b => b.IsAvailable);

            var ids = books.Select(b => b.BookId).ToList();
            var borrowCounts = ids.Count > 0
                ? await _bookRepository.GetBorrowedBooksCountAsync(ids) 
                : new Dictionary<int, int>();

            var dtos = _mapper.Map<List<BookDto>>(books);

            foreach (var dto in dtos)
                dto.BorrowCount = borrowCounts.TryGetValue(dto.BookId, out var c) ? c : 0;

            return dtos;
        }


        public async Task<IEnumerable<BookListDto>> GetAllBooksForListAsync(
            string? filter = null, bool includeUnavailable = true)
        {
            var booksQuery = _bookRepository.GetAllForList();

            if (!includeUnavailable)
            {
                booksQuery = booksQuery.Where(b => b.IsAvailable);
            }

            // Project to DTO
            var bookListDtos = booksQuery.Select(b => new BookListDto
            {
                BookId = b.BookId,
                Title = b.Title,
                PublicationYear = b.PublicationYear,
                IsAvailable = b.IsAvailable,
                AuthorId = b.AuthorId,
                AuthorName = b.Author != null ? b.Author.Name : null,
                PublisherId = b.PublisherId,
                PublisherName = b.Publisher != null ? b.Publisher.Name : null,

                CoverImageUrl = b.ImageFileName != null ? $"/api/Books/{b.BookId}/cover" : null
            });


            if (!string.IsNullOrWhiteSpace(filter))
            {
                var term = filter.Trim();
                var like = $"%{term}%";
                bool parsedYear = int.TryParse(term, out var year);

                bookListDtos = bookListDtos.Where(b =>
                    EF.Functions.Like(b.Title ?? "", like) ||
                    EF.Functions.Like(b.AuthorName ?? "", like) ||
                    EF.Functions.Like(b.PublisherName ?? "", like) ||
                    (parsedYear && b.PublicationYear == year)
                );
            }

            // artık burada materialize et
            var result = await bookListDtos.ToListAsync();

            return result;
        }


        public async Task UpdateBookAsync(int id, UpdateBookDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (id <= 0) throw new ArgumentException("Book ID must be greater than zero.", nameof(id));

            var book = await _bookRepository.GetByIdAsync(id);
            if (book == null) throw new BookNotFoundException(id);

            if (!Book.IsValidPublicationYear(dto.PublicationYear))
                throw new ValidationException("PublicationYear", $"Publication year {dto.PublicationYear} is not valid");

            if (!await _authorRepository.ExistsAsync(dto.AuthorId))
                throw new ValidationException("AuthorId", $"Author with ID {dto.AuthorId} does not exist");

            if (!await _publisherRepository.ExistsAsync(dto.PublisherId))
                throw new ValidationException("PublisherId", $"Publisher with ID {dto.PublisherId} does not exist");


            _mapper.Map(dto, book);

            if (dto.RemoveCoverImage)
            {
                book.RemoveCoverImage();
            }
            else if (dto.CoverImage != null)
            {
                await ProcessCoverImageAsync(book, dto.CoverImage);
            }

            await _bookRepository.SaveChangesAsync();

            _loggingService.LogCrudOperation("UPDATE", "Book", id, 0, new
            {
                dto.Title,
                dto.AuthorId,
                dto.PublisherId,
                dto.PublicationYear,
                dto.IsAvailable,
                dto.Rating,
                dto.CategoryId,
                dto.RemoveCoverImage,
                HasNewCoverImage = dto.CoverImage != null
            });
        }


        public async Task BorrowBookAsync(BorrowBookDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            // Custom validation
            if (!dto.IsValid(out string validationError))
                throw new ValidationException("BorrowData", validationError);

            // Get entities first
            var book = await _bookRepository.GetByIdAsync(dto.BookId);
            var user = await _userRepository.GetByIdAsync(dto.UserId);

            if (book == null)
                throw new BookNotFoundException(dto.BookId);

            if (user == null)
                throw new KeyNotFoundException($"User with ID {dto.UserId} not found.");

            // Check if book is available BEFORE trying to borrow
            if (!book.IsAvailable)
                throw new BookNotAvailableException(book.BookId, book.Title);

            // Use domain logic for borrowing
            book.MarkAsBorrowed();
            _bookRepository.Update(book);

            // Create loan record using domain constructor
            var loan = new Loan(dto.BookId, dto.UserId, dto.BorrowDate, dto.DueDate);

            await _loanRepository.AddAsync(loan);
            
            // Save all changes in one transaction - this ensures atomicity
            await _bookRepository.SaveChangesAsync();
            await _loanRepository.SaveChangesAsync();

            // Business logic logging
            _loggingService.LogBookAction(dto.UserId, "BORROW", dto.BookId, book.Title, new { 
                BorrowDate = dto.BorrowDate, 
                DueDate = dto.DueDate 
            });
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
            await _bookRepository.SaveChangesAsync();

            // Log CRUD operation
            _loggingService.LogCrudOperation("DELETE", "Book", id, 0, new { 
                Title = book.Title,
                AuthorId = book.AuthorId, 
                PublisherId = book.PublisherId,
                IsAvailable = book.IsAvailable
            });
        }

        public async Task ReturnBookAsync(int bookId, int userId)
        {
            // Get entities first
            var book = await _bookRepository.GetByIdAsync(bookId);
            var user = await _userRepository.GetByIdAsync(userId);

            if (book == null)
                throw new BookNotFoundException(bookId);

            if (user == null)
                throw new KeyNotFoundException($"User with ID {userId} not found.");

            // Find active loan for this specific user and book
            var activeLoan = await _loanRepository.GetActiveLoanAsync(bookId, userId);
            
            if (activeLoan == null)
                throw new InvalidOperationException($"No active loan found for book {bookId} and user {userId}");

            // Use domain logic
            book.MarkAsReturned();
            activeLoan.MarkAsReturned();

            _bookRepository.Update(book);
            _loanRepository.Update(activeLoan);
            
            await _bookRepository.SaveChangesAsync();
            await _loanRepository.SaveChangesAsync();

            // Business logic logging
            _loggingService.LogBookAction(userId, "RETURN", bookId, book.Title, new { 
                BorrowDate = activeLoan.LoanDate, 
                DueDate = activeLoan.DueDate,
                ReturnDate = DateTime.UtcNow 
            });
        }

        public async Task<bool> BookExistsAsync(int id)
        {
            return await _bookRepository.ExistsAsync(id);
        }

        public async Task<bool> IsBookBorrowedByUserAsync(int bookId, int userId)
        {
            if (bookId <= 0)
                throw new ArgumentException("Book ID must be greater than zero.", nameof(bookId));

            if (userId <= 0)
                throw new ArgumentException("User ID must be greater than zero.", nameof(userId));

            var activeLoan = await _loanRepository.GetActiveLoanAsync(bookId, userId);
            return activeLoan != null;
        }

        public async Task<IEnumerable<BookDto>> GetBooksByAuthorIdAsync(int authorId)
        {
            var authorExists = await _authorRepository.ExistsAsync(authorId);
            if (!authorExists)
            {
                throw new AuthorNotFoundException(authorId);
            }

            var books = await _bookRepository.GetAllAsync(includeNavigationProperties: true);
            var filteredBooks = books.Where(b => b.AuthorId == authorId);
            var dtos = _mapper.Map<IEnumerable<BookDto>>(filteredBooks);
            foreach (var dto in dtos)
                dto.BorrowCount = 0;
            return dtos;
        }

        public async Task<IEnumerable<BookDto>> GetBooksByPublisherIdAsync(int publisherId)
        {
            var publisherExists = await _publisherRepository.ExistsAsync(publisherId);
            if (!publisherExists)
            {
                throw new PublisherNotFoundException(publisherId);
            }

            var books = await _bookRepository.GetAllAsync(includeNavigationProperties: true);
            var filteredBooks = books.Where(b => b.PublisherId == publisherId);
            var dtos = _mapper.Map<IEnumerable<BookDto>>(filteredBooks);
            foreach (var dto in dtos)
                dto.BorrowCount = 0;
            return dtos;
        }



        // Favorite books methods
        public async Task<int> GetFavoriteCountAsync(int bookId)
        {
            if (bookId <= 0)
                throw new ArgumentException("Book ID must be greater than zero.", nameof(bookId));

            var book = await _bookRepository.GetByIdAsync(bookId, includeNavigationProperties: true);
            if (book == null)
                throw new BookNotFoundException(bookId);

            return book.GetFavoriteCount();
        }

        public async Task<bool> IsBookFavoritedByUserAsync(int bookId, int userId)
        {
            if (bookId <= 0)
                throw new ArgumentException("Book ID must be greater than zero.", nameof(bookId));

            if (userId <= 0)
                throw new ArgumentException("User ID must be greater than zero.", nameof(userId));

            var book = await _bookRepository.GetByIdAsync(bookId, includeNavigationProperties: true);
            if (book == null)
                throw new BookNotFoundException(bookId);

            return book.IsFavoritedByUser(userId);
        }

        public async Task<IEnumerable<UserDto>> GetUsersWhoFavoritedAsync(int bookId)
        {
            if (bookId <= 0)
                throw new ArgumentException("Book ID must be greater than zero.", nameof(bookId));

            var book = await _bookRepository.GetByIdAsync(bookId, includeNavigationProperties: true);
            if (book == null)
                throw new BookNotFoundException(bookId);

            var users = await _userRepository.GetUsersWhoFavoritedAsync(bookId);
            
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        /// <summary>
        /// Processes and sets the cover image for a book
        /// </summary>
        /// <param name="book">The book to set the cover image for</param>
        /// <param name="imageFile">The image file to process</param>
        private async Task ProcessCoverImageAsync(Book book, IFormFile imageFile)
        {
            // Validate file
            if (imageFile == null || imageFile.Length == 0)
            {
                throw new ValidationException("CoverImage", "Image file is required");
            }

            // Validate file size (max 5MB)
            if (imageFile.Length > 5 * 1024 * 1024)
            {
                throw new ValidationException("CoverImage", "Image file size must be less than 5MB");
            }

            // Validate file type
            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
            if (!allowedTypes.Contains(imageFile.ContentType.ToLower()))
            {
                throw new ValidationException("CoverImage", "Only JPEG, PNG, and GIF images are allowed");
            }

            // Read file into byte array
            using var memoryStream = new MemoryStream();
            await imageFile.CopyToAsync(memoryStream);
            var imageBytes = memoryStream.ToArray();

            // Set the cover image
            book.SetCoverImage(imageBytes, imageFile.ContentType, imageFile.FileName);
        }

        /// <summary>
        /// Gets the cover image for a book
        /// </summary>
        /// <param name="bookId">The book ID</param>
        /// <returns>Tuple containing image bytes, content type, and file name</returns>
        public async Task<(byte[] ImageBytes, string ContentType, string FileName)?> GetCoverImageAsync(int bookId)
        {
            var book = await _bookRepository.GetByIdAsync(bookId);
            if (book == null || !book.HasCoverImage())
            {
                return null;
            }

            return (book.CoverImage!, book.ImageContentType!, book.ImageFileName!);
        }

        /// <summary>
        /// Removes the cover image from a book
        /// </summary>
        /// <param name="bookId">The book ID</param>
        public async Task RemoveCoverImageAsync(int bookId)
        {
            var book = await _bookRepository.GetByIdAsync(bookId);
            if (book == null)
            {
                throw new BookNotFoundException(bookId);
            }

            book.RemoveCoverImage();
            await _bookRepository.SaveChangesAsync();
        }

        // Interface'de tanımlanan eksik metodlar
        public async Task<bool> AuthorExistsAsync(int authorId)
        {
            return await _authorRepository.ExistsAsync(authorId);
        }

        public async Task<bool> PublisherExistsAsync(int publisherId)
        {
            return await _publisherRepository.ExistsAsync(publisherId);
        }

        public async Task<IEnumerable<BookDto>> GetAuthorBooksAsync(int authorId)
        {
            return await GetBooksByAuthorIdAsync(authorId);
        }

        public async Task<IEnumerable<BookDto>> GetPublisherBooksAsync(int publisherId)
        {
            return await GetBooksByPublisherIdAsync(publisherId);
        }

        public async Task<object> GetBookBorrowedByUserAsync(int bookId, int userId)
        {
            var loan = await _loanRepository.GetActiveLoanAsync(bookId, userId);
            if (loan == null)
                return new { isBorrowed = false };

            return new
            {
                isBorrowed = true,
                loanId = loan.LoanId,
                loanDate = loan.LoanDate,
                dueDate = loan.DueDate,
                isOverdue = loan.IsOverdue()
            };
        }

        public async Task<object> GetBookStatusForUserAsync(int bookId, int userId)
        {
            var book = await _bookRepository.GetByIdAsync(bookId);
            if (book == null)
                throw new BookNotFoundException(bookId);

            var loan = await _loanRepository.GetActiveLoanAsync(bookId, userId);
            var isFavorited = await IsBookFavoritedByUserAsync(bookId, userId);

            return new
            {
                bookId = bookId,
                isAvailable = book.IsAvailable,
                isBorrowedByUser = loan != null,
                isFavoritedByUser = isFavorited,
                loanInfo = loan != null ? new
                {
                    loanId = loan.LoanId,
                    loanDate = loan.LoanDate,
                    dueDate = loan.DueDate,
                    isOverdue = loan.IsOverdue()
                } : null
            };
        }

        public async Task<Dictionary<int, object>> GetBookStatusForUserBatchAsync(int[] bookIds, int userId)
        {
            var result = new Dictionary<int, object>();
            
            foreach (var bookId in bookIds)
            {
                try
                {
                    var status = await GetBookStatusForUserAsync(bookId, userId);
                    result[bookId] = status;
                }
                catch (BookNotFoundException)
                {
                    // Kitap bulunamadıysa null olarak ekle
                    result[bookId] = null;
                }
            }
            
            return result;
        }

        public async Task<int> GetBookFavoriteCountAsync(int bookId)
        {
            return await GetFavoriteCountAsync(bookId);
        }

        public async Task<IEnumerable<UserDto>> GetUsersWhoFavoritedBookAsync(int bookId)
        {
            return await GetUsersWhoFavoritedAsync(bookId);
        }

        public async Task<(byte[]? content, string contentType, string fileName)> GetBookCoverAsync(int bookId)
        {
            var result = await GetCoverImageAsync(bookId);
            if (result == null)
                return (null, "", "");

            return (result.Value.ImageBytes, result.Value.ContentType, result.Value.FileName);
        }

        public async Task DeleteBookCoverAsync(int bookId)
        {
            await RemoveCoverImageAsync(bookId);
        }

        // Interface'de tanımlanan eksik metod
        public async Task BorrowBookAsync(int bookId, int userId)
        {
            var dto = new BorrowBookDto
            {
                BookId = bookId,
                UserId = userId,
                BorrowDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(14) // 2 hafta
            };

            await BorrowBookAsync(dto);
        }
    }
}