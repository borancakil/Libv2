using LibraryApp.Domain.Enums;

namespace LibraryApp.Domain.Entities
{
    public class User
    {
        public int UserId { get; set; } 
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; } // Hashed password
        public int? Age { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public DateTime RegistrationDate { get; private set; } 
        public UserRole Role { get; private set; } 

        public ICollection<Loan> BorrowedBooks { get; set; } = new List<Loan>();

        public User()
        {
            RegistrationDate = DateTime.UtcNow;
            Role = UserRole.User;
        }

        public void PromoteToAdmin()
        {
            this.Role = UserRole.Admin;
        }

        public void DemoteToUser()
        {
            this.Role = UserRole.User;
        }
    }
}