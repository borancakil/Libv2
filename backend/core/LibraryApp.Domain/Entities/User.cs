using LibraryApp.Domain.Enums;

namespace LibraryApp.Domain.Entities
{
    /// <summary>
    /// Represents a user in the library system
    /// </summary>
    public class User
    {
        // Parameterless constructor for EF Core
        private User() 
        {
            RegistrationDate = DateTime.UtcNow;
            Role = UserRole.User;
            BorrowedBooks = new List<Loan>();
        }

        public int UserId { get; set; }

        private string _name = string.Empty;
        public string Name 
        { 
            get => _name;
            set 
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Name cannot be null or empty.", nameof(value));
                _name = value.Trim();
            }
        }

        private string _email = string.Empty;
        public string Email 
        { 
            get => _email;
            set 
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Email cannot be null or empty.", nameof(value));
                if (!IsValidEmail(value))
                    throw new ArgumentException("Email format is invalid.", nameof(value));
                _email = value.ToLowerInvariant().Trim();
            }
        }

        private string _password = string.Empty;
        public string Password 
        { 
            get => _password;
            set 
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Password cannot be null or empty.", nameof(value));
                _password = value;
            }
        }

        public int? Age { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public DateTime RegistrationDate { get; private set; }
        public UserRole Role { get; private set; }

        public ICollection<Loan> BorrowedBooks { get; set; } = new List<Loan>();

        // Public constructors
        public User(string name, string email, string password) : this()
        {
            Name = name;
            Email = email;
            Password = password;
        }

        public User(string name, string email, string password, int? age, string? gender, string? address) 
            : this(name, email, password)
        {
            Age = age;
            Gender = gender;
            Address = address;
        }

        // --- Domain Logic Methods ---

        /// <summary>
        /// Promotes user to admin role
        /// </summary>
        public void PromoteToAdmin()
        {
            Role = UserRole.Admin;
        }

        /// <summary>
        /// Demotes user to regular user role
        /// </summary>
        public void DemoteToUser()
        {
            Role = UserRole.User;
        }

        /// <summary>
        /// Updates user's password with validation
        /// </summary>
        /// <param name="newPassword">New password</param>
        public void UpdatePassword(string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
                throw new ArgumentException("Password cannot be null or empty.", nameof(newPassword));
            
            Password = newPassword;
        }

        /// <summary>
        /// Updates user profile information
        /// </summary>
        /// <param name="name">New name</param>
        /// <param name="email">New email</param>
        /// <param name="age">New age (optional)</param>
        /// <param name="gender">New gender (optional)</param>
        /// <param name="address">New address (optional)</param>
        public void UpdateProfile(string name, string email, int? age = null, string? gender = null, string? address = null)
        {
            Name = name;     // Will trigger validation in setter
            Email = email;   // Will trigger validation in setter
            Age = age;
            Gender = gender;
            Address = address;
        }

        /// <summary>
        /// Gets active (not returned) loans for this user
        /// </summary>
        /// <returns>Collection of active loans</returns>
        public IEnumerable<Loan> GetActiveLoans()
        {
            return BorrowedBooks.Where(loan => loan.ReturnDate == null);
        }

        /// <summary>
        /// Checks if user can borrow more books
        /// </summary>
        /// <param name="maxBooksAllowed">Maximum books allowed per user</param>
        /// <returns>True if user can borrow more books</returns>
        public bool CanBorrowBooks(int maxBooksAllowed = 5)
        {
            var activeLoanCount = GetActiveLoans().Count();
            return activeLoanCount < maxBooksAllowed;
        }

        /// <summary>
        /// Checks if user has overdue books
        /// </summary>
        /// <returns>True if user has overdue books</returns>
        public bool HasOverdueBooks()
        {
            var today = DateTime.UtcNow.Date;
            return GetActiveLoans().Any(loan => loan.DueDate.Date < today);
        }

        /// <summary>
        /// Validates email format using basic regex
        /// </summary>
        /// <param name="email">Email to validate</param>
        /// <returns>True if email format is valid</returns>
        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}