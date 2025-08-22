using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryApp.Domain.Entities;


namespace LibraryApp.Domain.Interfaces
{
    public interface IAuthorRepository
    {
        Task<Author?> GetByIdAsync(int id, bool includeNavigationProperties = false);

        Task<IEnumerable<Author>> GetAllAsync(bool includeNavigationProperties = false);

        Task AddAsync(Author author);

        void Update(Author author);

        void Delete(Author author);

        Task<bool> ExistsAsync(int id);

        Task<int> SaveChangesAsync();
    }
}
