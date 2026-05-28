using CurrencyApp.Application.Contracts;
using CurrencyApp.Domain.Common;
using CurrencyApp.Domain.Entities;
using CurrencyApp.Domain.Interfaces;

namespace CurrencyApp.Application.Services;

public class ExchangeRateService : IExchangeRateService
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(30);
    private readonly IExchangeRateCacheRepository _exchangeRateCacheRepository;
    private readonly IExchangeRateProvider _exchangeRateProvider;
    private readonly IUnitOfWork _unitOfWork;

    public ExchangeRateService(IExchangeRateCacheRepository exchangeRateCacheRepository, IExchangeRateProvider exchangeRateProvider, IUnitOfWork unitOfWork)
    {
        _exchangeRateCacheRepository = exchangeRateCacheRepository;
        _exchangeRateProvider = exchangeRateProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<decimal> GetRateAsync(string fromCode, string toCode, CancellationToken ct = default)
    {
        var normalizedFromCode = CurrencyCodeRules.Normalize(fromCode);
        var normalizedToCode = CurrencyCodeRules.Normalize(toCode);

        if (normalizedFromCode == normalizedToCode)
            return 1;

        var now = DateTime.UtcNow;
        var cache = await _exchangeRateCacheRepository.GetValidAsync(normalizedFromCode, normalizedToCode, now);

        if (cache is not null)
            return cache.Rate;

        var result = await _exchangeRateProvider.FetchRateAsync(normalizedFromCode, normalizedToCode, ct);

        if (!result.Success)
            return 0;

        var rate = result.Rate;
        var expiresAt = now.Add(CacheTtl);

        var existingCache = await _exchangeRateCacheRepository.GetByPairAsync(normalizedFromCode, normalizedToCode);
        if (existingCache is not null)
        {
            existingCache.Refresh(rate, expiresAt);
            _exchangeRateCacheRepository.Update(existingCache);
        }
        else
        {
            var newCache = new ExchangeRateCache(normalizedFromCode, normalizedToCode, rate, expiresAt);
            await _exchangeRateCacheRepository.AddAsync(newCache);
        }

        await _unitOfWork.CommitAsync(ct);

        return rate;
    }
}
