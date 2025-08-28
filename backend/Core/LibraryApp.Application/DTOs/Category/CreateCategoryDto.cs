namespace LibraryApp.Application.DTOs.Category
{
    /// <summary>
    /// DTO for creating a new category
    /// </summary>
    public class CreateCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        
        public string? Description { get; set; }
    }
} 