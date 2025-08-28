using AutoMapper;
using LibraryApp.Domain.Entities;
using LibraryApp.Application.DTOs.Category;

namespace LibraryApp.Application.Mappings
{
    public class CategoryMappingProfile : Profile
    {
        public CategoryMappingProfile()
        {
            CreateMap<Category, CategoryDto>()
                .ForMember(d => d.CategoryId, o => o.MapFrom(s => s.CategoryId))
                .ForMember(d => d.Name, o => o.MapFrom(s => s.Name))
                .ForMember(d => d.Description, o => o.MapFrom(s => s.Description))
                .ForMember(d => d.BookCount, o => o.MapFrom(s => s.GetBookCount()));

            CreateMap<CreateCategoryDto, Category>()
                .ForCtorParam("name", o => o.MapFrom(s => s.Name))
                .ForCtorParam("description", o => o.MapFrom(s => s.Description));

            CreateMap<UpdateCategoryDto, Category>()
                .ForMember(d => d.CategoryId, o => o.Ignore());
        }
    }
}

