using Books.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Books.Api.Tests;

public class TestDbContextFactory : IDbContextFactory<BooksDbContext>
{
    private readonly DbContextOptions<BooksDbContext> _options;

    public TestDbContextFactory(string? databaseName = null)
    {
        var resolvedDatabaseName = databaseName ?? Guid.NewGuid().ToString();
        _options = new DbContextOptionsBuilder<BooksDbContext>()
            .UseInMemoryDatabase(resolvedDatabaseName)
            .Options;
    }

    public BooksDbContext CreateDbContext()
    {
        var booksDbContext = new BooksDbContext(_options);
        booksDbContext.Database.EnsureCreated();
        return booksDbContext;
    }
}

