using AutoMapper;
using LibraryApp.Domain.Entities;
using LibraryApp.Application.Features.Books.Commands.CreateBook;
using LibraryApp.Application.Features.Books.Queries.GetAllBooks;
using LibraryApp.Application.Features.Users.Commands.Register;

namespace LibraryApp.Application.Common.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // CREATE BOOK
            CreateMap<CreateBookCommand, Book>();

            // GET ALL BOOKS
            CreateMap<Book, BookDto>();

            // CREATE USER
            CreateMap<UserRegisterCommand, User>()
                .ForMember(dest => dest.Password, opt => opt.Ignore());

        }

    }
}