namespace Books.Api.Models;

public class PagedBooksResponse
{
    public List<Book> Items { get; init; } = [];
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }
    public string SortBy { get; init; } = string.Empty;
    public string SortOrder { get; init; } = string.Empty;
}
