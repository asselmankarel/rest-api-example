namespace Movies.Contracts.Responses;

public class PagedResponse<TResponse>
{
    public required IEnumerable<TResponse> Items { get; init; } = Enumerable.Empty<TResponse>();
    public required int Page { get; set; }
    public required int  PageSize { get; set; }
    public required int Total { get; set; }
    public bool HasNextPage => Total > (Page * PageSize);
}