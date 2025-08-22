namespace LibraryApp.Application.DTOs.Category
{
    /// <summary>
    /// DTO for updating an existing category
    /// </summary>
    public class UpdateCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        
        public string? Description { get; set; }
    }
} 