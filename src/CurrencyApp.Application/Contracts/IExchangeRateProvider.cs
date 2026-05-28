namespace CurrencyApp.Application.Contracts;

public interface IExchangeRateProvider
{
    Task<(bool Success, decimal Rate)> FetchRateAsync(string fromCode, string toCode, CancellationToken ct = default);
}
