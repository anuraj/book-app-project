using Books.Api.Models;

namespace Books.Api.Services;

public static class BookApiEndpoints
{
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
            logger.LogInformation("Creating book with title: {Title}", book.Title);
            var createdBook = await bookCollection.CreateBookAsync(book, cancellationToken);
            return Results.Created($"/books/{createdBook.Title}", createdBook);
        });

        api.MapPost("/bulk", async (List<Book> books, BookCollection bookCollection, CancellationToken cancellationToken) =>
        {
            logger.LogInformation("Creating multiple books");
            var createdBooks = await bookCollection.CreateBooksAsync(books, cancellationToken);
            return Results.Created("/books/bulk", createdBooks);
        });

        api.MapPut("/{title}", async (string title, Book book, BookCollection bookCollection, CancellationToken cancellationToken) =>
        {
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
}