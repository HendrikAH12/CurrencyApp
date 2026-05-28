using CurrencyApp.Domain.Common;
using CurrencyApp.Domain.Entities;
using CurrencyApp.Domain.Interfaces;
using CurrencyApp.Infra.Context;
using Microsoft.EntityFrameworkCore;

namespace CurrencyApp.Infra.Repositories;

public class ExchangeRateCacheRepository : IExchangeRateCacheRepository
{
    private readonly AppDbContext _context;

    public ExchangeRateCacheRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ExchangeRateCache?> GetValidAsync(string fromCode, string toCode, DateTime utcNow)
    {
        var normalizedFromCode = CurrencyCodeRules.Normalize(fromCode);
        var normalizedToCode = CurrencyCodeRules.Normalize(toCode);

        return await _context.ExchangeRateCaches
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.FromCode == normalizedFromCode && x.ToCode == normalizedToCode && x.ExpiresAtUtc > utcNow);
    }

    public async Task<ExchangeRateCache?> GetByPairAsync(string fromCode, string toCode)
    {
        var normalizedFromCode = CurrencyCodeRules.Normalize(fromCode);
        var normalizedToCode = CurrencyCodeRules.Normalize(toCode);

        return await _context.ExchangeRateCaches
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.FromCode == normalizedFromCode && x.ToCode == normalizedToCode);
    }

    public async Task AddAsync(ExchangeRateCache cache)
    {
        await _context.ExchangeRateCaches.AddAsync(cache);
    }

    public void Update(ExchangeRateCache cache)
    {
        _context.ExchangeRateCaches.Update(cache);
    }
}
