using AutoMapper;
using FluentValidation;
using MediatR;
using LibraryApp.Domain.Entities;
using LibraryApp.Domain.Interfaces;

namespace LibraryApp.Application.Features.Books.Commands.CreateBook
{
    public class CreateBookCommand : IRequest<int>
    {
        public string Title { get; set; }
        public int AuthorId { get; set; }
        public int PublisherId { get; set; }
        public int PublicationYear { get; set; }
    }


    public class CreateBookCommandHandler : IRequestHandler<CreateBookCommand, int>
    {
        private readonly IBookRepository _bookRepository;
        private readonly IMapper _mapper;

        public CreateBookCommandHandler(IBookRepository bookRepository, IMapper mapper)
        {
            _bookRepository = bookRepository;
            _mapper = mapper;
        }

        public async Task<int> Handle(CreateBookCommand request, CancellationToken cancellationToken)
        {
            var bookEntity = _mapper.Map<Book>(request);

            await _bookRepository.AddAsync(bookEntity);

            await _bookRepository.SaveChangesAsync(); 

            return bookEntity.BookId;
        }
    }


    public class CreateBookCommandValidator : AbstractValidator<CreateBookCommand>
    {
        public CreateBookCommandValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title cannot be empty.")
                .MaximumLength(200).WithMessage("Title can be at most 200 characters long.");

            RuleFor(x => x.AuthorId)
                .GreaterThan(0).WithMessage("A valid Author ID must be provided.");

            RuleFor(x => x.PublisherId)
                .GreaterThan(0).WithMessage("A valid Publisher ID must be provided.");
        }
    }
}