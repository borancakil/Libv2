using AutoMapper;
using LibraryApp.Domain.Entities;
using LibraryApp.Application.DTOs.Author;
using LibraryApp.Application.DTOs.Book;

namespace LibraryApp.Application.Mappings
{
    public class AuthorMappingProfile : Profile
    {
        public AuthorMappingProfile()
        {
            CreateMap<Author, AuthorDto>()
                .ForMember(d => d.AuthorId, o => o.MapFrom(s => s.AuthorId))
                .ForMember(d => d.Name, o => o.MapFrom(s => s.Name))
                .ForMember(d => d.Biography, o => o.MapFrom(s => s.Biography))
                .ForMember(d => d.Nationality, o => o.MapFrom(s => s.Nationality))
                .ForMember(d => d.BirthDate, o => o.MapFrom(s => s.BirthDate))
                .ForMember(d => d.HasProfileImage, o => o.MapFrom(s => s.HasProfileImage()))
                .ForMember(d => d.BookCount, o => o.MapFrom(s => s.GetBookCount()));

            CreateMap<Author, AuthorListDto>()
                .ForMember(d => d.AuthorId, o => o.MapFrom(s => s.AuthorId))
                .ForMember(d => d.Name, o => o.MapFrom(s => s.Name))
                .ForMember(d => d.Nationality, o => o.MapFrom(s => s.Nationality))
                .ForMember(d => d.BirthDate, o => o.MapFrom(s => s.BirthDate))
                .ForMember(d => d.BookCount, o => o.MapFrom(s => s.GetBookCount()))
                .ForMember(d => d.ProfileImageUrl, o => o.MapFrom(s => s.HasProfileImage() ? $"/api/Authors/{s.AuthorId}/profile-image" : null));

            CreateMap<CreateAuthorDto, Author>()
                .ForCtorParam("name", o => o.MapFrom(s => s.Name))
                .ForAllMembers(o => o.Ignore());

            CreateMap<UpdateAuthorDto, Author>()
                .ForMember(d => d.AuthorId, o => o.Ignore());

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

