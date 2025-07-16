using LibraryApp.Domain.Entities;

namespace LibraryApp.Domain.Interfaces
{
    /// <summary>
    /// Represents the database applications for Book Object.
    /// </summary>
    public interface IBookRepository
    {
        Task<Book> GetByIdAsync(int id);
        Task<IEnumerable<Book>> GetAllAsync();

        // POST
        Task AddAsync(Book book);

        Task<int> SaveChangesAsync();
        // PUT
        void Update(Book book);
        // DELETE
        void Delete(Book book);

    }
}