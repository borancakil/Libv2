using MediatR;
using LibraryApp.Domain.Interfaces;
using AutoMapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace LibraryApp.Application.Features.Books.Queries.GetAllBooks
{
    // This class will represent the data we return for each book.
    public class BookDto
    {
        public int BookId { get; set; }
        public string Title { get; set; }
        public bool IsAvailable { get; set; }
    }

    // The query itself. It doesn't need any parameters to get all books.
    public class GetAllBooksQuery : IRequest<List<BookDto>>
    {
    }

    // The handler that processes the query.
    public class GetAllBooksQueryHandler : IRequestHandler<GetAllBooksQuery, List<BookDto>>
    {
        private readonly IBookRepository _bookRepository;
        private readonly IMapper _mapper;

        public GetAllBooksQueryHandler(IBookRepository bookRepository, IMapper mapper)
        {
            _bookRepository = bookRepository;
            _mapper = mapper;
        }

        public async Task<List<BookDto>> Handle(GetAllBooksQuery request, CancellationToken cancellationToken)
        {
            var books = await _bookRepository.GetAllAsync();
            return _mapper.Map<List<BookDto>>(books);
        }
    }
}