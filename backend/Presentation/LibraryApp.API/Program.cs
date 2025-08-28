using FluentValidation;
using FluentValidation.AspNetCore;
using LibraryApp.Infrastructure.Auth;
using LibraryApp.Infrastructure.Logging;
using LibraryApp.Infrastructure.Middleware;
using LibraryApp.Application.Interfaces;
using LibraryApp.Application.Services;
using LibraryApp.Application.Validators.Author;
using LibraryApp.Application.Validators.Book;
using LibraryApp.Application.Validators.Publisher;
using LibraryApp.Application.Validators.User;
using LibraryApp.Domain.Interfaces;
using LibraryApp.Persistence.Data;
using LibraryApp.Persistence.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// HttpContextAccessor ekleme - JWT service için gerekli
builder.Services.AddHttpContextAccessor();

// Add Serilog configuration
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        Path.Combine("Logs", "app-.log"),
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
        retainedFileCountLimit: 7, // Reduce to 7 days
        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information, // Only log Information and above
        buffered: true, // Enable buffering for better performance
        flushToDiskInterval: TimeSpan.FromSeconds(5) // Flush every 5 seconds
    ));

// Add Entity Framework
builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
           .EnableSensitiveDataLogging(false) // Disable sensitive data logging for production
           .EnableDetailedErrors(false) // Disable detailed errors for production
           .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking) // Disable change tracking by default
           .ConfigureWarnings(warnings => warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.CoreEventId.NavigationBaseIncludeIgnored))); // Ignore navigation warnings

// JWT Authentication
builder.Services.AddAuthentication("Bearer")
    .AddScheme<AuthenticationSchemeOptions, EncryptedJwtHandler>("Bearer", options => { });

builder.Services.AddAuthorization();

// Add FluentValidation
builder.Services.AddFluentValidationAutoValidation()
                .AddFluentValidationClientsideAdapters();

// Register validators
builder.Services.AddValidatorsFromAssemblyContaining<CreateBookDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<AddUserDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateAuthorDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreatePublisherDtoValidator>();

// Register AutoMapper (Application katmanı profilleri)
builder.Services.AddAutoMapper(typeof(LibraryApp.Application.Mappings.BookMappingProfile).Assembly);

// Register repositories (Domain interfaces in Infrastructure layer)
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ILoanRepository, LoanRepository>();
builder.Services.AddScoped<IAuthorRepository, AuthorRepository>();
builder.Services.AddScoped<IPublisherRepository, PublisherRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

// Register services (Application layer)
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IUserService, UserService>();

// Infrastructure services
builder.Services.AddSingleton<IJwtService, JwtService>();
builder.Services.AddSingleton<ILoggingService, LoggingService>();

// Application services
builder.Services.AddScoped<IAuthorService, AuthorService>();
builder.Services.AddScoped<IPublisherService, PublisherService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "LibraryApp API", Version = "v1" });
    
    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT token'ınızı direkt yapıştırın. Bearer prefix'i otomatik eklenecektir."
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
    
    // Add XML comments if available
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// CORS konfigürasyonu ekle
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Middleware pipeline - order is important!
app.UseMiddleware<RequestLoggingMiddleware>();     // 1. Request logging (first)
app.UseMiddleware<GlobalExceptionMiddleware>();    // 2. Exception handling

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// HTTPS redirection kaldırıldı - HTTP'ye izin ver
// app.UseHttpsRedirection();

// Add static files middleware for serving uploaded files
app.UseStaticFiles();

// CORS middleware ekle
app.UseCors("AllowAll");

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Database will be managed manually - no auto-creation

app.Run();
