using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using LibraryApp.Application.Features.Users.Commands.Register;
using LibraryApp.Application.Features.Users.Commands.UpdateUser;
using LibraryApp.Application.Features.Users.Queries.Login;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using LibraryApp.Application.DTOs;
using LibraryApp.Application.Features.Users.Queries.GetAllUsers;

namespace LibraryApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator)
        {
            _mediator = mediator;
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginQuery query)
        {
            try
            {
                var token = await _mediator.Send(query);
                return Ok(new { Token = token });
            }
            catch (ValidationException ex)
            {
                // Return 400 Bad Request for validation errors (e.g., wrong password)
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpPost("register")] 
        public async Task<IActionResult> Register([FromBody] UserRegisterCommand command)
        {
            try
            {
                var userId = await _mediator.Send(command);
                return Ok(new { UserId = userId, Message = "User registered successfully." });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { Errors = ex.Errors.Select(e => e.ErrorMessage) });
            }
        }


        [HttpPut("{id}/role")]
        [Authorize]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateUserRoleCommand command)
        {
            // Ensure the ID from the route matches the ID in the command body for consistency.
            if (id != command.UserId)
            {
                return BadRequest("Route ID and Command ID do not match.");
            }

            await _mediator.Send(command);

            // Return 204 NoContent, which is the standard response for a successful PUT/DELETE
            // that doesn't return any data.
            return NoContent();
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<List<UserDto>>> GetAllUsers()
        {
            var result = await _mediator.Send(new GetAllUsersQuery());
            return Ok(result);
        }
    }
}