namespace ELTE.Cinema.DataAccess.Models;

public record PaginatedResult<T>(
    int Total,
    int PageSize,
    int CurrentPage,
    int TotalPages,
    IReadOnlyCollection<T> Items
);