using CurrencyApp.Domain.Entities;

namespace CurrencyApp.Domain.Interfaces;

public interface IExchangeRateCacheRepository
{
    Task<ExchangeRateCache?> GetValidAsync(string fromCode, string toCode, DateTime utcNow);

    Task<ExchangeRateCache?> GetByPairAsync(string fromCode, string toCode);

    Task AddAsync(ExchangeRateCache cache);

    void Update(ExchangeRateCache cache);
}
