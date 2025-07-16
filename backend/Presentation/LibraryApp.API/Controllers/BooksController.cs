using MediatR;
using Microsoft.AspNetCore.Mvc;
using LibraryApp.Application.Features.Books.Commands.CreateBook;
using System.Threading.Tasks;
using LibraryApp.Application.Features.Books.Queries.GetAllBooks;
using LibraryApp.Application.Features.Books.Commands.UpdateBook;
using LibraryApp.Application.Features.Books.Commands.DeleteBook;

namespace LibraryApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BooksController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBookCommand command)
        {
            var bookId = await _mediator.Send(command);

            return Ok(bookId);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var books = await _mediator.Send(new GetAllBooksQuery());
            return Ok(books);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateBookCommand command)
        {
            if (id != command.BookId)
            {
                return BadRequest("ID mismatch");
            }
            await _mediator.Send(command);
            return NoContent(); 
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _mediator.Send(new DeleteBookCommand { BookId = id });
            return NoContent(); // 204 No Content is also standard for successful deletion.
        }

    }
}