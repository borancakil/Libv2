using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryApp.Application.DTOs.Book;
using LibraryApp.Application.DTOs.User;

namespace LibraryApp.Application.Interfaces
{
    public interface IBookService
    {
        Task<int> AddBookAsync(CreateBookDto dto);
        
        Task<BookDto?> GetByIdAsync(int id);
        
        Task<IEnumerable<BookDto>> GetAllBooksAsync(string? filter, bool includeUnavailable = true);
        
        Task<IEnumerable<BookListDto>> GetAllBooksForListAsync(string? filter = null, bool includeUnavailable = true);



        Task UpdateBookAsync(int id, UpdateBookDto dto);
        
        Task BorrowBookAsync(BorrowBookDto dto);
        Task BorrowBookAsync(int bookId, int userId);
        
        Task DeleteBookAsync(int id);
        
        Task ReturnBookAsync(int bookId, int userId);
        
        Task<bool> BookExistsAsync(int id);
        Task<bool> IsBookBorrowedByUserAsync(int bookId, int userId);
        Task<IEnumerable<BookDto>> GetBooksByAuthorIdAsync(int authorId);
        Task<IEnumerable<BookDto>> GetBooksByPublisherIdAsync(int publisherId);

        // Author and Publisher methods
        Task<bool> AuthorExistsAsync(int authorId);
        Task<bool> PublisherExistsAsync(int publisherId);
        Task<IEnumerable<BookDto>> GetAuthorBooksAsync(int authorId);
        Task<IEnumerable<BookDto>> GetPublisherBooksAsync(int publisherId);

        // Book status methods
        Task<object> GetBookBorrowedByUserAsync(int bookId, int userId);
        Task<object> GetBookStatusForUserAsync(int bookId, int userId);
        Task<Dictionary<int, object>> GetBookStatusForUserBatchAsync(int[] bookIds, int userId);

        // Favorite books methods
        Task<int> GetFavoriteCountAsync(int bookId);
        Task<bool> IsBookFavoritedByUserAsync(int bookId, int userId);
        Task<IEnumerable<UserDto>> GetUsersWhoFavoritedAsync(int bookId);
        Task<int> GetBookFavoriteCountAsync(int bookId);
        Task<IEnumerable<UserDto>> GetUsersWhoFavoritedBookAsync(int bookId);

        // Cover image methods
        Task<(byte[] ImageBytes, string ContentType, string FileName)?> GetCoverImageAsync(int bookId);
        Task RemoveCoverImageAsync(int bookId);
        Task<(byte[]? content, string contentType, string fileName)> GetBookCoverAsync(int bookId);
        Task DeleteBookCoverAsync(int bookId);
    }

}
