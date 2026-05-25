using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Books.Api.Data;
using Books.Api.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Books.Api.Tests;

public class BookApiEndpointsTests
{
    [Fact]
    public async Task GetBooks_ReturnsEmptyList_WhenNoBooksExist()
    {
        await using var factory = new BooksApiFactory();
        await EnsureDatabaseCreatedAsync(factory);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

        var response = await client.GetAsync("/books/");

        response.EnsureSuccessStatusCode();
        var books = await response.Content.ReadFromJsonAsync<List<Book>>();
        Assert.NotNull(books);
        Assert.Empty(books);
    }

    [Fact]
    public async Task GetBooksByTitle_ReturnsNotFound_WhenBookDoesNotExist()
    {
        await using var factory = new BooksApiFactory();
        await EnsureDatabaseCreatedAsync(factory);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

        var response = await client.GetAsync("/books/missing-title");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetBooksByTitle_ReturnsBook_WhenBookExists()
    {
        await using var factory = new BooksApiFactory();
        await EnsureDatabaseCreatedAsync(factory);
        await SeedBookAsync(factory, new Book { Title = "Dune", Author = "Frank Herbert", Year = 1965, Read = true });
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

        var response = await client.GetAsync("/books/Dune");

        response.EnsureSuccessStatusCode();
        var book = await response.Content.ReadFromJsonAsync<Book>();
        Assert.NotNull(book);
        Assert.Equal("Dune", book.Title);
        Assert.Equal("Frank Herbert", book.Author);
    }

    [Fact]
    public async Task CreateBook_ReturnsCreated_AndPersistsBook()
    {
        await using var factory = new BooksApiFactory();
        await EnsureDatabaseCreatedAsync(factory);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

        var newBook = new Book { Title = "Neuromancer", Author = "William Gibson", Year = 1984, Read = false };

        var response = await client.PostAsJsonAsync("/books/", newBook);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("/books/Neuromancer", response.Headers.Location?.OriginalString);

        await using var scope = factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<BooksDbContext>();
        var savedBook = await context.Books.SingleAsync(b => b.Title == "Neuromancer");
        Assert.Equal("William Gibson", savedBook.Author);
    }

    [Fact]
    public async Task CreateBooksBulk_ReturnsCreated_AndPersistsAllBooks()
    {
        await using var factory = new BooksApiFactory();
        await EnsureDatabaseCreatedAsync(factory);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

        var books = new List<Book>
        {
            new() { Title = "Book A", Author = "Author A", Year = 2000 },
            new() { Title = "Book B", Author = "Author B", Year = 2001 }
        };

        var response = await client.PostAsJsonAsync("/books/bulk", books);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("/books/bulk", response.Headers.Location?.OriginalString);

        await using var scope = factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<BooksDbContext>();
        Assert.Equal(2, await context.Books.CountAsync());
    }

    [Fact]
    public async Task CreateBook_ReturnsBadRequest_WhenPayloadIsInvalid()
    {
        await using var factory = new BooksApiFactory();
        await EnsureDatabaseCreatedAsync(factory);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

        var response = await client.PostAsJsonAsync("/books/", new Book
        {
            Title = " ",
            Author = "",
            Year = 1200
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = await ReadValidationErrorsAsync(response);
        Assert.Contains(nameof(Book.Title), errors.Keys);
        Assert.Contains(nameof(Book.Author), errors.Keys);
        Assert.Contains(nameof(Book.Year), errors.Keys);
    }

    [Fact]
    public async Task UpdateBook_ReturnsBadRequest_WhenUpdatedFieldsAreInvalid()
    {
        await using var factory = new BooksApiFactory();
        await EnsureDatabaseCreatedAsync(factory);
        await SeedBookAsync(factory, new Book { Title = "Foundation", Author = "Old Author", Year = 1951 });
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

        var response = await client.PutAsJsonAsync("/books/Foundation", new Book
        {
            Author = " ",
            Year = 1200
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = await ReadValidationErrorsAsync(response);
        Assert.Contains(nameof(Book.Author), errors.Keys);
        Assert.Contains(nameof(Book.Year), errors.Keys);
    }

    [Fact]
    public async Task CreateBooksBulk_ReturnsBadRequest_AndPersistsNothing_WhenAnyBookIsInvalid()
    {
        await using var factory = new BooksApiFactory();
        await EnsureDatabaseCreatedAsync(factory);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

        var books = new List<Book>
        {
            new() { Title = "Book A", Author = "Author A", Year = 2000 },
            new() { Title = "", Author = "Author B", Year = 2001 }
        };

        var response = await client.PostAsJsonAsync("/books/bulk", books);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = await ReadValidationErrorsAsync(response);
        Assert.Contains("Books[1].Title", errors.Keys);

        await using var scope = factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<BooksDbContext>();
        Assert.Equal(0, await context.Books.CountAsync());
    }

    [Fact]
    public async Task UpdateBook_ReturnsNotFound_WhenBookDoesNotExist()
    {
        await using var factory = new BooksApiFactory();
        await EnsureDatabaseCreatedAsync(factory);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

        var response = await client.PutAsJsonAsync("/books/unknown", new Book { Author = "Updated", Year = 2000 });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_ReturnsNoContent_AndUpdatesBook_WhenBookExists()
    {
        await using var factory = new BooksApiFactory();
        await EnsureDatabaseCreatedAsync(factory);
        await SeedBookAsync(factory, new Book { Title = "Foundation", Author = "Old Author", Year = 1951 });
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

        var response = await client.PutAsJsonAsync("/books/Foundation", new Book { Author = "Isaac Asimov", Year = 1950 });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        await using var scope = factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<BooksDbContext>();
        var updatedBook = await context.Books.SingleAsync(b => b.Title == "Foundation");
        Assert.Equal("Isaac Asimov", updatedBook.Author);
        Assert.Equal(1950, updatedBook.Year);
    }

    [Fact]
    public async Task MarkBookAsRead_ReturnsNotFound_WhenBookDoesNotExist()
    {
        await using var factory = new BooksApiFactory();
        await EnsureDatabaseCreatedAsync(factory);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

        var response = await client.PutAsync("/books/unknown/read", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task MarkBookAsRead_ReturnsNoContent_AndPersistsReadStatus_WhenBookExists()
    {
        await using var factory = new BooksApiFactory();
        await EnsureDatabaseCreatedAsync(factory);
        await SeedBookAsync(factory, new Book { Title = "The Hobbit", Author = "J.R.R. Tolkien", Year = 1937, Read = false });
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

        var response = await client.PutAsync("/books/The Hobbit/read", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        await using var scope = factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<BooksDbContext>();
        var updatedBook = await context.Books.SingleAsync(b => b.Title == "The Hobbit");
        Assert.True(updatedBook.Read);
        Assert.Equal("J.R.R. Tolkien", updatedBook.Author);
        Assert.Equal(1937, updatedBook.Year);
    }

    [Fact]
    public async Task DeleteBook_ReturnsNotFound_WhenBookDoesNotExist()
    {
        await using var factory = new BooksApiFactory();
        await EnsureDatabaseCreatedAsync(factory);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

        var response = await client.DeleteAsync("/books/unknown");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteBook_ReturnsNoContent_AndRemovesBook_WhenBookExists()
    {
        await using var factory = new BooksApiFactory();
        await EnsureDatabaseCreatedAsync(factory);
        await SeedBookAsync(factory, new Book { Title = "Hyperion", Author = "Dan Simmons", Year = 1989 });
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

        var response = await client.DeleteAsync("/books/Hyperion");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        await using var scope = factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<BooksDbContext>();
        Assert.Empty(context.Books);
    }

    [Fact]
    public async Task OpenApiEndpoint_IsAvailable_InDevelopment()
    {
        await using var factory = new BooksApiFactory();
        await EnsureDatabaseCreatedAsync(factory);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

        var response = await client.GetAsync("/openapi/v1.json");

        response.EnsureSuccessStatusCode();
    }

    private static async Task EnsureDatabaseCreatedAsync(BooksApiFactory factory)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<BooksDbContext>();
        await context.Database.EnsureCreatedAsync();
    }

    private static async Task SeedBookAsync(BooksApiFactory factory, Book book)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<BooksDbContext>();
        await context.Database.EnsureCreatedAsync();
        context.Books.Add(book);
        await context.SaveChangesAsync();
    }

    private static async Task<Dictionary<string, string[]>> ReadValidationErrorsAsync(HttpResponseMessage response)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        var json = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(json);

        if (!document.RootElement.TryGetProperty("errors", out var errorElement))
        {
            return errors;
        }

        foreach (var property in errorElement.EnumerateObject())
        {
            errors[property.Name] = property.Value
                .EnumerateArray()
                .Select(value => value.GetString() ?? string.Empty)
                .ToArray();
        }

        return errors;
    }

    private sealed class BooksApiFactory : WebApplicationFactory<Program>
    {
        private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"books-api-tests-{Guid.NewGuid():N}.db");

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment(Environments.Development);
            builder.ConfigureAppConfiguration((_, configBuilder) =>
            {
                configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:BooksDbContext"] = $"Data Source={_databasePath}"
                });
            });
        }
    }
}
