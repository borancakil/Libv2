using LibraryApp.Domain.Enums;

namespace LibraryApp.Application.DTOs.User
{
    /// <summary>
    /// DTO representing a user for read operations
    /// </summary>
    public class UserDto
    {
        public int UserId { get; set; }
        
        public string Name { get; set; } = string.Empty;
        
        public string Email { get; set; } = string.Empty;
        
        public UserRole Role { get; set; }
        
        public DateTime RegistrationDate { get; set; }
        
        /// <summary>
        /// Number of currently active book loans
        /// </summary>
        public int ActiveLoansCount { get; set; }
        
        /// <summary>
        /// Whether the user has any overdue books
        /// </summary>
        public bool HasOverdueBooks { get; set; }
        
        /// <summary>
        /// User's age (optional)
        /// </summary>
        public int? Age { get; set; }
        
        /// <summary>
        /// User's gender (optional)
        /// </summary>
        public string? Gender { get; set; }
        
        /// <summary>
        /// User's address (optional)
        /// </summary>
        public string? Address { get; set; }
    }
}
