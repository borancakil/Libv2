using MediatR;
using LibraryApp.Domain.Interfaces;
using System.Threading.Tasks;
using System.Threading;

namespace LibraryApp.Application.Features.Books.Commands.DeleteBook
{
    public class DeleteBookCommand : IRequest<Unit>
    {
        public int BookId { get; set; }
    }

    public class DeleteBookCommandHandler : IRequestHandler<DeleteBookCommand, Unit>
    {
        private readonly IBookRepository _bookRepository;

        public DeleteBookCommandHandler(IBookRepository bookRepository)
        {
            _bookRepository = bookRepository;
        }

        public async Task<Unit> Handle(DeleteBookCommand request, CancellationToken cancellationToken)
        {
            var bookToDelete = await _bookRepository.GetByIdAsync(request.BookId);
            if (bookToDelete == null)
            {
                // Burada istersen NotFoundException fırlatabilirsin
                return Unit.Value;
            }

            _bookRepository.Delete(bookToDelete);
            await _bookRepository.SaveChangesAsync();

            return Unit.Value;
        }
    }
}
