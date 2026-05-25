var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres");
var booksDatabase = postgres.AddDatabase("booksdb");

builder.AddProject<Projects.Books_Api>("books-api")
    .WithReference(booksDatabase)
    .WaitFor(postgres);

builder.Build().Run();
