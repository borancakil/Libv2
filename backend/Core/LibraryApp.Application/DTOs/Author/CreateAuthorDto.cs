using Microsoft.AspNetCore.Http;

namespace LibraryApp.Application.DTOs.Author
{
    public class CreateAuthorDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Biography { get; set; }
        public string? Nationality { get; set; }
        public IFormFile? ProfileImage { get; set; }
    }
} 