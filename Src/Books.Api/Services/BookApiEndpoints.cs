using Books.Api.Models;

namespace Books.Api.Services;

public static class BookApiEndpoints
{
    private const int MinPublicationYear = 1450;

    public static void MapBookApiEndpoints(this WebApplication app)
    {
        var logger = app.Logger;
        var api = app.MapGroup("/books");
        api.MapGet("/", async (BookCollection bookCollection, CancellationToken cancellationToken) =>
        {
            logger.LogInformation("Getting all books");
            var books = await bookCollection.ReadBooksAsync(cancellationToken);
            return Results.Ok(books);
        });

        api.MapGet("/{title}", async (string title, BookCollection bookCollection, CancellationToken cancellationToken) =>
        {
            logger.LogInformation("Getting book with title: {Title}", title);
            var book = await bookCollection.ReadBookAsync(title, cancellationToken);
            if (book is null)
            {
                logger.LogWarning("Book with title: {Title} not found", title);
                return Results.NotFound();
            }
            return Results.Ok(book);
        });

        api.MapPost("/", async (Book book, BookCollection bookCollection, CancellationToken cancellationToken) =>
        {
            var errors = ValidateCreateBook(book);
            if (errors.Count > 0)
            {
                return Results.ValidationProblem(errors);
            }

            logger.LogInformation("Creating book with title: {Title}", book.Title);
            var createdBook = await bookCollection.CreateBookAsync(book, cancellationToken);
            return Results.Created($"/books/{createdBook.Title}", createdBook);
        });

        api.MapPost("/bulk", async (List<Book> books, BookCollection bookCollection, CancellationToken cancellationToken) =>
        {
            var errors = ValidateBulkBooks(books);
            if (errors.Count > 0)
            {
                return Results.ValidationProblem(errors);
            }

            logger.LogInformation("Creating multiple books");
            var createdBooks = await bookCollection.CreateBooksAsync(books, cancellationToken);
            return Results.Created("/books/bulk", createdBooks);
        });

        api.MapPut("/{title}", async (string title, Book book, BookCollection bookCollection, CancellationToken cancellationToken) =>
        {
            var errors = ValidateUpdateBook(book);
            if (errors.Count > 0)
            {
                return Results.ValidationProblem(errors);
            }

            logger.LogInformation("Updating book with title: {Title}", title);
            var existingBook = await bookCollection.ReadBookAsync(title, cancellationToken);
            if (existingBook is null)
            {
                logger.LogWarning("Book with title: {Title} not found", title);
                return Results.NotFound();
            }

            existingBook.Author = book.Author;
            existingBook.Year = book.Year;

            await bookCollection.UpdateBookAsync(existingBook, cancellationToken);
            return Results.NoContent();
        });

        api.MapDelete("/{title}", async (string title, BookCollection bookCollection, CancellationToken cancellationToken) =>
        {
            logger.LogInformation("Deleting book with title: {Title}", title);
            var existingBook = await bookCollection.ReadBookAsync(title, cancellationToken);
            if (existingBook is null)
            {
                logger.LogWarning("Book with title: {Title} not found", title);
                return Results.NotFound();
            }

            await bookCollection.DeleteBookAsync(existingBook, cancellationToken);
            return Results.NoContent();
        });
    }

    private static Dictionary<string, string[]> ValidateCreateBook(Book book)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        ValidateRequiredTextField(nameof(Book.Title), book.Title, errors);
        ValidateRequiredTextField(nameof(Book.Author), book.Author, errors);
        ValidateYearField(book.Year, nameof(Book.Year), errors);
        return errors;
    }

    private static Dictionary<string, string[]> ValidateUpdateBook(Book book)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        ValidateRequiredTextField(nameof(Book.Author), book.Author, errors);
        ValidateYearField(book.Year, nameof(Book.Year), errors);
        return errors;
    }

    private static Dictionary<string, string[]> ValidateBulkBooks(List<Book> books)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        if (books.Count == 0)
        {
            AddError(errors, "Books", "At least one book is required.");
            return errors;
        }

        for (var index = 0; index < books.Count; index++)
        {
            var book = books[index];
            ValidateRequiredTextField($"Books[{index}].{nameof(Book.Title)}", book.Title, errors);
            ValidateRequiredTextField($"Books[{index}].{nameof(Book.Author)}", book.Author, errors);
            ValidateYearField(book.Year, $"Books[{index}].{nameof(Book.Year)}", errors);
        }

        return errors;
    }

    private static void ValidateRequiredTextField(string fieldName, string value, Dictionary<string, string[]> errors)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        AddError(errors, fieldName, $"{fieldName} is required.");
    }

    private static void ValidateYearField(int year, string fieldName, Dictionary<string, string[]> errors)
    {
        if (year >= MinPublicationYear && year <= DateTime.UtcNow.Year)
        {
            return;
        }

        AddError(errors, fieldName, $"{fieldName} must be between {MinPublicationYear} and {DateTime.UtcNow.Year}.");
    }

    private static void AddError(Dictionary<string, string[]> errors, string fieldName, string message)
    {
        if (errors.TryGetValue(fieldName, out var existingMessages))
        {
            errors[fieldName] = [.. existingMessages, message];
            return;
        }

        errors[fieldName] = [message];
    }
}