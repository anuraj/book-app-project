using Books.Api.Data;
using Books.Api.Services;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<BooksDbContext>((serviceProvider, options) =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var databaseProvider = configuration["DatabaseProvider"];

    if (string.Equals(databaseProvider, "InMemory", StringComparison.OrdinalIgnoreCase))
    {
        var inMemoryDatabaseName = configuration["InMemoryDatabaseName"] ?? "books-api";
        options.UseInMemoryDatabase(inMemoryDatabaseName);
        return;
    }

    options.UseNpgsql(
        configuration.GetConnectionString("booksdb")
        ?? configuration.GetConnectionString("BooksDbContext")
        ?? "Host=localhost;Port=5432;Database=booksdb;Username=postgres;Password=postgres");
});

builder.Services.AddOpenApi();

builder.Services.AddScoped<BookCollection>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.MapBookApiEndpoints();

app.Run();

public partial class Program { }
