using Books.Api.Data;
using Books.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Books.Api.Services;

public class BookCollection(BooksDbContext booksDbContext, ILogger<BookCollection> logger)
{
    private readonly BooksDbContext _booksDbContext = booksDbContext;
    private readonly ILogger<BookCollection> _logger = logger;

    public async Task<List<Book>> ReadBooksAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Reading all books from the database");
        var books = await _booksDbContext.Books.ToListAsync(cancellationToken);
        return books;
    }

    public async Task<Book?> ReadBookAsync(string title, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Reading book with title: {Title} from the database", title);
        var book = await _booksDbContext.Books.FirstOrDefaultAsync(b => b.Title == title, cancellationToken);
        return book;
    }

    public async Task<Book> CreateBookAsync(Book book, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating book with title: {Title} in the database", book.Title);
        _booksDbContext.Books.Add(book);
        await _booksDbContext.SaveChangesAsync(cancellationToken);
        return book;
    }

    public async Task UpdateBookAsync(Book book, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating book with title: {Title} in the database", book.Title);
        _booksDbContext.Books.Update(book);
        await _booksDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteBookAsync(Book book, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting book with title: {Title} from the database", book.Title);
        _booksDbContext.Books.Remove(book);
        await _booksDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<Book>> CreateBooksAsync(List<Book> books, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating multiple books in the database - Count: {Count}", books.Count);
        _booksDbContext.Books.AddRange(books);
        var inserted = await _booksDbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Inserted {Count} books into the database", inserted);
        return books;
    }
}