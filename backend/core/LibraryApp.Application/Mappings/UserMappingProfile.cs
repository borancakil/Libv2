using AutoMapper;
using LibraryApp.Domain.Entities;
using LibraryApp.Application.DTOs.User;
using LibraryApp.Application.DTOs.Book;

namespace LibraryApp.Application.Mappings
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            CreateMap<User, UserDto>()
                .ForMember(d => d.UserId, o => o.MapFrom(s => s.UserId))
                .ForMember(d => d.Name, o => o.MapFrom(s => s.Name))
                .ForMember(d => d.Email, o => o.MapFrom(s => s.Email))
                .ForMember(d => d.Role, o => o.MapFrom(s => s.Role))
                .ForMember(d => d.RegistrationDate, o => o.MapFrom(s => s.RegistrationDate))
                .ForMember(d => d.ActiveLoansCount, o => o.MapFrom(s => s.GetActiveLoans().Count()))
                .ForMember(d => d.HasOverdueBooks, o => o.MapFrom(s => s.HasOverdueBooks()));

            CreateMap<Book, BookDto>()
                .ForMember(d => d.AuthorName, o => o.MapFrom(s => s.Author != null ? s.Author.Name : null))
                .ForMember(d => d.PublisherName, o => o.MapFrom(s => s.Publisher != null ? s.Publisher.Name : null))
                .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category != null ? s.Category.Name : null))
                .ForMember(d => d.CategoryDescription, o => o.MapFrom(s => s.Category != null ? s.Category.Description : null))
                .ForMember(d => d.HasCoverImage, o => o.MapFrom(s => s.HasCoverImage()))
                .ForMember(d => d.BorrowCount, o => o.Ignore());
        }
    }
}

