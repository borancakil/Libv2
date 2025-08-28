using System.ComponentModel.DataAnnotations;

namespace LibraryApp.Application.DTOs.Book
{
    public class ReturnBookDto
    {
        [Required]
        public int UserId { get; set; }
    }
} 