namespace LibraryApp.Application.Exceptions
{
    /// <summary>
    /// Exception thrown when a category is not found
    /// </summary>
    public class CategoryNotFoundException : Exception
    {
        public CategoryNotFoundException() : base("Category not found.")
        {
        }

        public CategoryNotFoundException(string message) : base(message)
        {
        }

        public CategoryNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
} 