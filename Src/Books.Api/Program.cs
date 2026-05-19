using Books.Api.Data;
using Books.Api.Services;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

SQLitePCL.Batteries.Init();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<BooksDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("BooksDbContext") ?? "Data Source=Data\\books.db"));

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
