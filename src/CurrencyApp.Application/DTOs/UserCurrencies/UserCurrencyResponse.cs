namespace CurrencyApp.Application.DTOs.UserCurrencies;

public sealed class UserCurrencyResponse
{
    public int UserId { get; init; }
    public int CurrencyId { get; init; }
    public string CurrencyCode { get; init; } = string.Empty;
    public decimal Amount { get; init; }
}
