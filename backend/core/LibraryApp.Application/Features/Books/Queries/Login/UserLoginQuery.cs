using MediatR;
using LibraryApp.Domain.Interfaces;
using FluentValidation;
using BCrypt.Net;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations; // For ValidationException
using Microsoft.Extensions.Configuration; // To read appsettings.json
using System.IdentityModel.Tokens.Jwt; // To create JWT
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System;
using LibraryApp.Domain.Entities; // For User entity

namespace LibraryApp.Application.Features.Users.Queries.Login
{
    /// <summary>
    /// The query that represents a user's login request.
    /// It returns a string, which will be the JWT.
    /// </summary>
    public class UserLoginQuery : IRequest<string>
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    /// <summary>
    /// The handler for the UserLoginQuery. It validates the user and generates a JWT.
    /// </summary>
    public class UserLoginQueryHandler : IRequestHandler<UserLoginQuery, string>
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public UserLoginQueryHandler(IUserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        public async Task<string> Handle(UserLoginQuery request, CancellationToken cancellationToken)
        {
            // 1. Find the user by email
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null)
            {
                throw new FluentValidation.ValidationException("Invalid email or password.");
            }

            // 2. Verify the password
            // The first argument is the plain text password from the request.
            // The second argument is the hashed password from the database.
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.Password);
            if (!isPasswordValid)
            {
                throw new FluentValidation.ValidationException("Invalid email or password.");
            }

            // 3. If password is valid, generate JWT
            return GenerateJwtToken(user);
        }

        private string GenerateJwtToken(User user)
        {
            // Claims are statements about an entity (e.g., a user)
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()), // Subject (unique user ID)
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // JWT ID (for tracking)
                new Claim(ClaimTypes.Role, user.Role.ToString()) // Add the user's role as a claim
            };

            // Get the secret key from appsettings.json
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            // Create signing credentials
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Define token expiration
            var expires = DateTime.UtcNow.AddDays(1); // Token is valid for 1 day

            // Create the token
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            // Write the token to a string
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    /// <summary>
    /// Validates the UserLoginQuery.
    /// </summary>
    public class UserLoginQueryValidator : AbstractValidator<UserLoginQuery>
    {
        public UserLoginQueryValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty();
        }
    }
}