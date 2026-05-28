namespace CurrencyApp.Application.Contracts;

public interface IExchangeRateService
{
    Task<decimal> GetRateAsync(string fromCode, string toCode, CancellationToken ct = default);
}
