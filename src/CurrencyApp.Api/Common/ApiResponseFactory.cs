namespace CurrencyApp.Api.Common;

public static class ApiResponseFactory
{
    public static object Data(object? result)
        => new { data = result };

    public static object PaginatedData<T>(IReadOnlyList<T> items, string? nextCursor)
        => new
        {
            data = items,
            pagination = new
            {
                nextCursor
            }
        };

    public static object Error(string? message)
        => new { error = new { message = message ?? "An unexpected error occurred." } };
}
