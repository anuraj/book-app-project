using Books.Api.Models;
using Books.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Books.Api.Tests;

public class BookCollectionTests
{
    [Fact]
    public async Task ReadBooksAsync_ReturnsAllBooks()
    {
        var databaseName = Guid.NewGuid().ToString();
        using var context = new TestDbContextFactory(databaseName).CreateDbContext();
        context.Books.AddRange(
            new Book { Title = "Book 1", Author = "Author 1", Year = 2020 },
            new Book { Title = "Book 2", Author = "Author 2", Year = 2021 });
        await context.SaveChangesAsync();

        var sut = new BookCollection(context, Substitute.For<ILogger<BookCollection>>());

        var books = await sut.ReadBooksAsync(CancellationToken.None);

        Assert.Equal(2, books.Count);
        Assert.Contains(books, b => b.Title == "Book 1");
        Assert.Contains(books, b => b.Title == "Book 2");
    }

    [Fact]
    public async Task ReadBookAsync_ReturnsMatchingBook()
    {
        var databaseName = Guid.NewGuid().ToString();
        using var context = new TestDbContextFactory(databaseName).CreateDbContext();
        context.Books.Add(new Book { Title = "Dune", Author = "Frank Herbert", Year = 1965 });
        await context.SaveChangesAsync();

        var sut = new BookCollection(context, Substitute.For<ILogger<BookCollection>>());

        var book = await sut.ReadBookAsync("Dune", CancellationToken.None);

        Assert.NotNull(book);
        Assert.Equal("Frank Herbert", book.Author);
    }

    [Fact]
    public async Task ReadBookAsync_ReturnsNull_WhenBookDoesNotExist()
    {
        var databaseName = Guid.NewGuid().ToString();
        using var context = new TestDbContextFactory(databaseName).CreateDbContext();

        var sut = new BookCollection(context, Substitute.For<ILogger<BookCollection>>());

        var book = await sut.ReadBookAsync("Missing", CancellationToken.None);

        Assert.Null(book);
    }

    [Fact]
    public async Task CreateBookAsync_PersistsBook()
    {
        var databaseName = Guid.NewGuid().ToString();
        using var context = new TestDbContextFactory(databaseName).CreateDbContext();
        var sut = new BookCollection(context, Substitute.For<ILogger<BookCollection>>());
        var newBook = new Book { Title = "Neuromancer", Author = "William Gibson", Year = 1984 };

        await sut.CreateBookAsync(newBook, CancellationToken.None);

        var savedBook = await context.Books.SingleAsync(b => b.Title == "Neuromancer");
        Assert.Equal("William Gibson", savedBook.Author);
        Assert.Equal(1984, savedBook.Year);
    }

    [Fact]
    public async Task UpdateBookAsync_UpdatesExistingBook()
    {
        var databaseName = Guid.NewGuid().ToString();
        using var context = new TestDbContextFactory(databaseName).CreateDbContext();
        var existingBook = new Book { Title = "Foundation", Author = "Author A", Year = 1951 };
        context.Books.Add(existingBook);
        await context.SaveChangesAsync();

        var sut = new BookCollection(context, Substitute.For<ILogger<BookCollection>>());

        existingBook.Author = "Isaac Asimov";
        existingBook.Year = 1950;

        await sut.UpdateBookAsync(existingBook, CancellationToken.None);

        var updatedBook = await context.Books.SingleAsync(b => b.Title == "Foundation");
        Assert.Equal("Isaac Asimov", updatedBook.Author);
        Assert.Equal(1950, updatedBook.Year);
    }

    [Fact]
    public async Task DeleteBookAsync_RemovesBook()
    {
        var databaseName = Guid.NewGuid().ToString();
        using var context = new TestDbContextFactory(databaseName).CreateDbContext();
        var existingBook = new Book { Title = "Hyperion", Author = "Dan Simmons", Year = 1989 };
        context.Books.Add(existingBook);
        await context.SaveChangesAsync();

        var sut = new BookCollection(context, Substitute.For<ILogger<BookCollection>>());

        await sut.DeleteBookAsync(existingBook, CancellationToken.None);

        Assert.Empty(context.Books);
    }

    [Fact]
    public async Task CreateBooksAsync_PersistsAllBooks()
    {
        var databaseName = Guid.NewGuid().ToString();
        using var context = new TestDbContextFactory(databaseName).CreateDbContext();
        var sut = new BookCollection(context, Substitute.For<ILogger<BookCollection>>());
        var booksToInsert = new List<Book>
        {
            new() { Title = "Book A", Author = "Author A", Year = 2000 },
            new() { Title = "Book B", Author = "Author B", Year = 2001 },
            new() { Title = "Book C", Author = "Author C", Year = 2002 }
        };

        var createdBooks = await sut.CreateBooksAsync(booksToInsert, CancellationToken.None);

        Assert.Equal(3, createdBooks.Count);
        Assert.Equal(3, await context.Books.CountAsync());
    }
}
