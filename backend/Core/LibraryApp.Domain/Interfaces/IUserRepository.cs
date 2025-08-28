using LibraryApp.Domain.Entities;

namespace LibraryApp.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id, bool includeNavigationProperties = false);
        Task<User?> GetByEmailAsync(string email, bool includeNavigationProperties = false);
        Task<IEnumerable<User>> GetAllAsync(bool includeNavigationProperties = false);
        Task AddAsync(User user);
        void Update(User user);
        void Delete(User user);
        Task<int> SaveChangesAsync();
        Task<bool> ExistsAsync(int id);
        Task<bool> EmailExistsAsync(string email);
        Task<IEnumerable<Book>> GetBorrowedBooksAsync(int userId, bool includeNavigationProperties = false);
        Task<IEnumerable<Book>> GetFavoriteBooksAsync(int userId, bool includeNavigationProperties = false);
        Task<IEnumerable<User>> GetUsersWhoFavoritedAsync(int bookId, bool includeNavigationProperties = false);
        Task<IEnumerable<Loan>> GetAllLoansAsync();
        Task AddFavoriteBookAsync(UserFavoriteBook userFavoriteBook);
        Task RemoveFavoriteBookAsync(UserFavoriteBook userFavoriteBook);
        Task<UserFavoriteBook?> GetFavoriteBookAsync(int userId, int bookId);
    }
}