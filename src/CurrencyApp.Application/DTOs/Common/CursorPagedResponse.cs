namespace CurrencyApp.Application.DTOs.Common;

public class CursorPagedResponse<T>
{
    public required IReadOnlyList<T> Items { get; init; }

    public string? NextCursor { get; init; }
}
