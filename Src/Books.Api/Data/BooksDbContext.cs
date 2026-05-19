using Books.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Books.Api.Data;

public class BooksDbContext : DbContext
{
    public BooksDbContext(DbContextOptions<BooksDbContext> options) : base(options)
    {
    }

    public DbSet<Book> Books { get; set; }
}