namespace CurrencyApp.Application.DTOs.Currencies;

public sealed class CurrencyResponse
{
    public int Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
}
