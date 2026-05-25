using Books.Api.Models;
using Books.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Books.Api.Tests;

public class BookCollectionTests
{
    [Fact]
    public async Task ReadBooksAsync_ReturnsPagedBooks_WithMetadata()
    {
        var databaseName = Guid.NewGuid().ToString();
        using var context = new TestDbContextFactory(databaseName).CreateDbContext();
        context.Books.AddRange(
            new Book { Title = "C", Author = "Author 3", Year = 2022 },
            new Book { Title = "A", Author = "Author 1", Year = 2020 },
            new Book { Title = "B", Author = "Author 2", Year = 2021 });
        await context.SaveChangesAsync();

        var sut = new BookCollection(context, Substitute.For<ILogger<BookCollection>>());

        var books = await sut.ReadBooksAsync(1, 2, "title", "asc", CancellationToken.None);

        Assert.Equal(2, books.Items.Count);
        Assert.Equal(["A", "B"], books.Items.Select(book => book.Title).ToArray());
        Assert.Equal(1, books.PageNumber);
        Assert.Equal(2, books.PageSize);
        Assert.Equal(3, books.TotalCount);
        Assert.Equal(2, books.TotalPages);
        Assert.Equal("title", books.SortBy);
        Assert.Equal("asc", books.SortOrder);
    }

    [Fact]
    public async Task ReadBooksAsync_SortsDescending_AndReturnsEmptyPage_WhenOutOfRange()
    {
        var databaseName = Guid.NewGuid().ToString();
        using var context = new TestDbContextFactory(databaseName).CreateDbContext();
        context.Books.AddRange(
            new Book { Title = "Book 1", Author = "Author 1", Year = 2020, Read = false },
            new Book { Title = "Book 2", Author = "Author 2", Year = 2021, Read = true },
            new Book { Title = "Book 3", Author = "Author 3", Year = 2022, Read = false });
        await context.SaveChangesAsync();

        var sut = new BookCollection(context, Substitute.For<ILogger<BookCollection>>());

        var sortedBooks = await sut.ReadBooksAsync(1, 2, "year", "desc", CancellationToken.None);
        var emptyPage = await sut.ReadBooksAsync(3, 2, "year", "desc", CancellationToken.None);

        Assert.Equal([2022, 2021], sortedBooks.Items.Select(book => book.Year).ToArray());
        Assert.Empty(emptyPage.Items);
        Assert.Equal(3, emptyPage.TotalCount);
        Assert.Equal(2, emptyPage.TotalPages);
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
    public async Task MarkBookAsReadAsync_ReturnsFalse_WhenBookDoesNotExist()
    {
        var databaseName = Guid.NewGuid().ToString();
        using var context = new TestDbContextFactory(databaseName).CreateDbContext();
        var sut = new BookCollection(context, Substitute.For<ILogger<BookCollection>>());

        var markedAsRead = await sut.MarkBookAsReadAsync("Missing", CancellationToken.None);

        Assert.False(markedAsRead);
    }

    [Fact]
    public async Task MarkBookAsReadAsync_MarksBookAsRead_AndPersists()
    {
        var databaseName = Guid.NewGuid().ToString();
        using var context = new TestDbContextFactory(databaseName).CreateDbContext();
        context.Books.Add(new Book { Title = "Project Hail Mary", Author = "Andy Weir", Year = 2021, Read = false });
        await context.SaveChangesAsync();

        var sut = new BookCollection(context, Substitute.For<ILogger<BookCollection>>());

        var markedAsRead = await sut.MarkBookAsReadAsync("Project Hail Mary", CancellationToken.None);

        Assert.True(markedAsRead);
        var updatedBook = await context.Books.SingleAsync(b => b.Title == "Project Hail Mary");
        Assert.True(updatedBook.Read);
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
