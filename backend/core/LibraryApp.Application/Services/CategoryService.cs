using LibraryApp.Application.DTOs.Category;
using LibraryApp.Domain.Entities;
using LibraryApp.Domain.Interfaces;
using LibraryApp.Application.Exceptions;
using LibraryApp.Application.Interfaces;
using AutoMapper;

namespace LibraryApp.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILoggingService _loggingService;
        private readonly IMapper _mapper;

        public CategoryService(ICategoryRepository categoryRepository, ILoggingService loggingService, IMapper mapper)
        {
            _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
            _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<CategoryDto>>(categories);
        }

        public async Task<CategoryDto?> GetCategoryByIdAsync(int categoryId)
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId);
            if (category == null)
                return null;

            return _mapper.Map<CategoryDto>(category);
        }

        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto)
        {
            var category = _mapper.Map<Category>(createCategoryDto);
            var createdCategory = await _categoryRepository.AddAsync(category);
            await _categoryRepository.SaveChangesAsync();

            // Business logic logging
            _loggingService.LogUserAction(0, "CREATE_CATEGORY", $"Category created: {category.Name}", new { 
                Name = category.Name,
                Description = category.Description 
            });

            return _mapper.Map<CategoryDto>(createdCategory);
        }

        public async Task<CategoryDto> UpdateCategoryAsync(int categoryId, UpdateCategoryDto updateCategoryDto)
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId);
            if (category == null)
                throw new CategoryNotFoundException($"Category with ID {categoryId} not found.");

            _mapper.Map(updateCategoryDto, category);

            await _categoryRepository.SaveChangesAsync();

            // Business logic logging
            _loggingService.LogUserAction(0, "UPDATE_CATEGORY", $"Category updated: {category.Name}", new { 
                CategoryId = categoryId,
                Name = category.Name,
                Description = category.Description 
            });

            return _mapper.Map<CategoryDto>(category);
        }

        public async Task<bool> DeleteCategoryAsync(int categoryId)
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId);
            if (category == null)
                return false;

            if (category.HasBooks())
                throw new InvalidOperationException($"Cannot delete category '{category.Name}' because it has books.");

            await _categoryRepository.DeleteAsync(category);
            await _categoryRepository.SaveChangesAsync();

            // Business logic logging
            _loggingService.LogUserAction(0, "DELETE_CATEGORY", $"Category deleted: {category.Name}", new { 
                CategoryId = categoryId,
                Name = category.Name 
            });

            return true;
        }

        public async Task<IEnumerable<CategoryDto>> GetCategoriesWithBookCountAsync()
        {
            var categories = await _categoryRepository.GetAllWithBooksAsync();
            return _mapper.Map<IEnumerable<CategoryDto>>(categories);
        }
    }
} 