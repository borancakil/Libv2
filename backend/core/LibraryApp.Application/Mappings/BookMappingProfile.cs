using AutoMapper;
using LibraryApp.Domain.Entities;
using LibraryApp.Application.DTOs.Book;

namespace LibraryApp.Application.Mappings
{
    public class BookMappingProfile : Profile
    {
        public BookMappingProfile()
        {
            // =============== Book -> BookListDto ===============
            CreateMap<Book, BookListDto>()
                .ForMember(d => d.BookId,
                    o => o.MapFrom(s => s.BookId))
                .ForMember(d => d.Title,
                    o => o.MapFrom(s => s.Title ?? "")) 
                .ForMember(d => d.PublicationYear,
                    o => o.MapFrom(s => (int?)s.PublicationYear)) 
                .ForMember(d => d.AuthorName,
                    o => o.MapFrom(s => s.Author != null ? s.Author.Name : ""))
                .ForMember(d => d.PublisherName,
                    o => o.MapFrom(s => s.Publisher != null ? s.Publisher.Name : ""))
                .ForMember(d => d.CategoryName,
                    o => o.MapFrom(s => s.Category != null ? s.Category.Name : ""))
                .ForMember(d => d.IsAvailable,
                    o => o.MapFrom(s => s.IsAvailable));

            // =============== Book -> BookDto (detay görünüm) ===============
            CreateMap<Book, BookDto>()
                .ForMember(d => d.BookId, o => o.MapFrom(s => s.BookId))
                .ForMember(d => d.Title, o => o.MapFrom(s => s.Title ?? ""))
                .ForMember(d => d.PublicationYear, o => o.MapFrom(s => s.PublicationYear))
                .ForMember(d => d.AuthorName, o => o.MapFrom(s => s.Author != null ? s.Author.Name : null))
                .ForMember(d => d.PublisherName, o => o.MapFrom(s => s.Publisher != null ? s.Publisher.Name : null))
                .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category != null ? s.Category.Name : null))
                .ForMember(d => d.CategoryDescription,
                    o => o.MapFrom(s => s.Category != null ? s.Category.Description : null))
                .ForMember(d => d.HasCoverImage,
                    o => o.MapFrom(s =>
                        (s.ImageFileName != null && s.ImageContentType != null) || s.CoverImage != null))
                .ForMember(d => d.BorrowCount, o => o.Ignore());



            // =============== UpdateBookDto -> Book ===============
            CreateMap<UpdateBookDto, Book>()
                // Kimlik ve navigation/görsel alanları dokunma
                .ForMember(d => d.BookId, o => o.Ignore())
                .ForMember(d => d.Category, o => o.Ignore())
                .ForMember(d => d.Author, o => o.Ignore())
                .ForMember(d => d.Publisher, o => o.Ignore())

                // Domain metodu gerektiren veya private set’li alanlar:
                .ForMember(d => d.IsAvailable, o => o.Ignore())
                .ForMember(d => d.Rating, o => o.Ignore())
                .ForMember(d => d.CategoryId, o => o.Ignore())

                // Kapak görseli EF tarafından da yükleniyor olabilir: proje akışına göre serviste yönet
                .ForMember(d => d.CoverImage, o => o.Ignore())
                .ForMember(d => d.ImageContentType, o => o.Ignore())
                .ForMember(d => d.ImageFileName, o => o.Ignore())

                // Geri kalan scalar alanlar (Title, PublicationYear, AuthorId, PublisherId) otomatik map’lenir
                .AfterMap((src, dest) =>
                {
                    // Domain kurallarıyla güncelle
                    dest.SetAvailability(src.IsAvailable);
                    dest.UpdateRating(src.Rating);
                    dest.UpdateCategory(src.CategoryId);
                });

        }
    }
}
