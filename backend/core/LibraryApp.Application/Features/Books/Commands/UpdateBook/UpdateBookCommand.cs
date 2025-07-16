using MediatR;
using LibraryApp.Domain.Interfaces;
using System.Threading.Tasks;
using System.Threading;

namespace LibraryApp.Application.Features.Books.Commands.UpdateBook
{
    public class UpdateBookCommand : IRequest<Unit> 
    {
        public int BookId { get; set; }
        public string Title { get; set; }
    }

    public class UpdateBookCommandHandler : IRequestHandler<UpdateBookCommand, Unit>
    {
        private readonly IBookRepository _bookRepository;

        public UpdateBookCommandHandler(IBookRepository bookRepository)
        {
            _bookRepository = bookRepository;
        }

        public async Task<Unit> Handle(UpdateBookCommand request, CancellationToken cancellationToken)
        {
            var bookToUpdate = await _bookRepository.GetByIdAsync(request.BookId);
            if (bookToUpdate == null)
            {
                // Hata fırlatmak daha sağlıklı olur:
                throw new KeyNotFoundException("Book not found.");
            }

            bookToUpdate.Title = request.Title;

            _bookRepository.Update(bookToUpdate);
            await _bookRepository.SaveChangesAsync();

            return Unit.Value;
        }
    }
}