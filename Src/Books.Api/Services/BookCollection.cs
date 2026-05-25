using Books.Api.Data;
using Books.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Books.Api.Services;

public class BookCollection(BooksDbContext booksDbContext, ILogger<BookCollection> logger)
{
    private readonly BooksDbContext _booksDbContext = booksDbContext;
    private readonly ILogger<BookCollection> _logger = logger;

    public async Task<PagedBooksResponse> ReadBooksAsync(int pageNumber, int pageSize, string sortBy, string sortOrder, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Reading books from the database. PageNumber: {PageNumber}, PageSize: {PageSize}, SortBy: {SortBy}, SortOrder: {SortOrder}",
            pageNumber,
            pageSize,
            sortBy,
            sortOrder);

        var booksQuery = _booksDbContext.Books.AsNoTracking();
        var totalCount = await booksQuery.CountAsync(cancellationToken);
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

        var books = await ApplySorting(booksQuery, sortBy, sortOrder)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedBooksResponse
        {
            Items = books,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            SortBy = sortBy,
            SortOrder = sortOrder
        };
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

    public async Task<bool> MarkBookAsReadAsync(string title, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Marking book with title: {Title} as read in the database", title);
        var book = await _booksDbContext.Books.FirstOrDefaultAsync(b => b.Title == title, cancellationToken);
        if (book is null)
        {
            return false;
        }

        book.Read = true;
        await _booksDbContext.SaveChangesAsync(cancellationToken);
        return true;
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

    private static IOrderedQueryable<Book> ApplySorting(IQueryable<Book> booksQuery, string sortBy, string sortOrder)
    {
        return (sortBy, sortOrder) switch
        {
            ("author", "desc") => booksQuery.OrderByDescending(book => book.Author).ThenBy(book => book.Id),
            ("author", _) => booksQuery.OrderBy(book => book.Author).ThenBy(book => book.Id),
            ("year", "desc") => booksQuery.OrderByDescending(book => book.Year).ThenBy(book => book.Id),
            ("year", _) => booksQuery.OrderBy(book => book.Year).ThenBy(book => book.Id),
            ("read", "desc") => booksQuery.OrderByDescending(book => book.Read).ThenBy(book => book.Id),
            ("read", _) => booksQuery.OrderBy(book => book.Read).ThenBy(book => book.Id),
            ("title", "desc") => booksQuery.OrderByDescending(book => book.Title).ThenBy(book => book.Id),
            _ => booksQuery.OrderBy(book => book.Title).ThenBy(book => book.Id)
        };
    }
}