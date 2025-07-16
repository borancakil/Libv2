// These are the using statements you already have
using FluentValidation;
using LibraryApp.Application.Common.Mappings;
using LibraryApp.Application.Features.Books.Commands.CreateBook;
using LibraryApp.Application.Features.Users.Commands.Register;
using LibraryApp.Domain.Interfaces;
using LibraryApp.Persistence.Data;
using LibraryApp.Persistence.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

// --- Add these using statements for JWT ---
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using System.Reflection; // Required for Swagger JWT configuration

var builder = WebApplication.CreateBuilder(args);

// --- START: Add services to the container ---

// 1. Configure DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. Configure Repositories
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// 3. Configure Application Layer Services
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);
builder.Services.AddMediatR(typeof(CreateBookCommand).Assembly);
builder.Services.AddValidatorsFromAssembly(typeof(UserRegisterCommand).Assembly);


// --- JWT AUTHENTICATION/AUTHORIZATION CONFIGURATION START ---

// 4. Configure Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],

        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],

        ValidateLifetime = true,

        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

// 5. Add Authorization services (this is often implicitly added but it's good to be explicit)
builder.Services.AddAuthorization();

// --- JWT AUTHENTICATION/AUTHORIZATION CONFIGURATION END ---


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// --- SWAGGER JWT CONFIGURATION START ---
// This part enables you to test protected endpoints by adding a JWT token in the Swagger UI.
builder.Services.AddSwaggerGen(options =>
{
    // Add a security definition for JWT Bearer tokens
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });

    // Make sure Swagger uses the security definition
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[]{}
        }
    });
});
// --- SWAGGER JWT CONFIGURATION END ---


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// --- ENABLE AUTHENTICATION/AUTHORIZATION MIDDLEWARE ---
// The order is crucial here. UseAuthentication must come before UseAuthorization.
app.UseAuthentication();
app.UseAuthorization(); // This was already here, but now it has a configured authentication scheme to work with.
// ---

app.MapControllers();

app.Run();