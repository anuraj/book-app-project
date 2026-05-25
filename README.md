# Book App Project

A small ASP.NET Core minimal API for managing a book collection. The API stores data in SQLite with Entity Framework Core and includes automated unit and integration tests.

## Features

- List all books
- Get a book by title
- Create a single book
- Create multiple books in bulk
- Update a book's author and publication year
- Mark a book as read
- Delete a book
- OpenAPI and Scalar API docs in development

## Tech Stack

| Area | Technology |
| --- | --- |
| Runtime | .NET 10 |
| API | ASP.NET Core Minimal APIs |
| Data access | Entity Framework Core |
| Database | SQLite |
| API docs | Microsoft.AspNetCore.OpenApi + Scalar |
| Tests | xUnit, ASP.NET Core integration testing, Coverlet |

## Repository Layout

```text
.
|-- Books.slnx
|-- Src/
|   `-- Books.Api/
`-- Tests/
    `-- Books.Api.Tests/
```

## Prerequisites

- .NET 10 SDK

## Getting Started

1. Restore dependencies:

   ```bash
   dotnet restore Books.slnx
   ```

2. Create or update the SQLite database before the first run:

   ```bash
   dotnet ef database update --project Src/Books.Api/Books.Api.csproj --startup-project Src/Books.Api/Books.Api.csproj
   ```

3. Run the API:

   ```bash
   dotnet run --project Src/Books.Api/Books.Api.csproj
   ```

4. In development, open:

- Scalar UI: `https://localhost:<port>/scalar`
- OpenAPI document: `https://localhost:<port>/openapi/v1.json`

## Configuration

The API reads the database connection string from `ConnectionStrings:BooksDbContext`.

- Environment variable: `ConnectionStrings__BooksDbContext`
- Default fallback: `Data Source=Data\books.db`

Example:

```bash
ConnectionStrings__BooksDbContext="Data Source=/tmp/books.db" dotnet run --project Src/Books.Api/Books.Api.csproj
```

## API Endpoints

| Method | Route | Description |
| --- | --- | --- |
| `GET` | `/books/` | Get all books |
| `GET` | `/books/{title}` | Get a single book by title |
| `POST` | `/books/` | Create one book |
| `POST` | `/books/bulk` | Create multiple books |
| `PUT` | `/books/{title}` | Update author and year |
| `PUT` | `/books/{title}/read` | Mark a book as read |
| `DELETE` | `/books/{title}` | Delete a book |

## Request Model

```json
{
  "title": "Dune",
  "author": "Frank Herbert",
  "year": 1965,
  "read": false
}
```

Validation rules:

- `title` is required when creating a book
- `author` is required
- `year` must be between `1450` and the current UTC year
- Bulk create requires at least one book

## Example Requests

Create a book:

```bash
curl -X POST https://localhost:5001/books/ \
  -H "Content-Type: application/json" \
  -d '{"title":"Dune","author":"Frank Herbert","year":1965,"read":false}'
```

Mark a book as read:

```bash
curl -X PUT https://localhost:5001/books/Dune/read
```

## Running Tests

Run the full test suite from the repository root:

```bash
dotnet test --nologo
```

The test project enforces a 90% total line coverage threshold with Coverlet.
