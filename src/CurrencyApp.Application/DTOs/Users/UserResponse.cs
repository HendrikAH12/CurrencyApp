using CurrencyApp.Application.DTOs.UserCurrencies;

namespace CurrencyApp.Application.DTOs.Users;

public sealed class UserResponse
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public int? MainCurrencyId { get; init; }
    public string? MainCurrencyCode { get; init; }
    public IReadOnlyList<UserCurrencyResponse> Holdings { get; init; } = [];
    public decimal? TotalInMainCurrency { get; set; }
}
