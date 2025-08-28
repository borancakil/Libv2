using AutoMapper;
using LibraryApp.Domain.Entities;
using LibraryApp.Application.DTOs.Publisher;
using LibraryApp.Application.DTOs.Book;

namespace LibraryApp.Application.Mappings
{
    public class PublisherMappingProfile : Profile
    {
        public PublisherMappingProfile()
        {
            CreateMap<Publisher, PublisherDto>()
                .ForMember(d => d.PublisherId, o => o.MapFrom(s => s.PublisherId))
                .ForMember(d => d.Name, o => o.MapFrom(s => s.Name))
                .ForMember(d => d.Address, o => o.MapFrom(s => s.Address))
                .ForMember(d => d.Website, o => o.MapFrom(s => s.Website))
                .ForMember(d => d.ContactEmail, o => o.MapFrom(s => s.ContactEmail))
                .ForMember(d => d.EstablishedDate, o => o.MapFrom(s => s.EstablishedDate))
                .ForMember(d => d.HasLogoImage, o => o.MapFrom(s => s.HasLogoImage()))
                .ForMember(d => d.BookCount, o => o.MapFrom(s => s.GetBookCount()));

            CreateMap<CreatePublisherDto, Publisher>()
                .ForCtorParam("name", o => o.MapFrom(s => s.Name))
                .ForAllMembers(o => o.Ignore());

            CreateMap<UpdatePublisherDto, Publisher>()
                .ForMember(d => d.PublisherId, o => o.Ignore());

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

