namespace LibraryApp.Application.DTOs.Category
{
    /// <summary>
    /// DTO representing a category for read operations
    /// </summary>
    public class CategoryDto
    {
        public int CategoryId { get; set; }
        
        public string Name { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        /// <summary>
        /// Number of books in this category
        /// </summary>
        public int BookCount { get; set; }
    }
} 