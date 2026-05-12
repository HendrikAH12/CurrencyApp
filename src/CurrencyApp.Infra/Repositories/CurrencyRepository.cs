using CurrencyApp.Domain.Entities;
using CurrencyApp.Domain.Interfaces;
using CurrencyApp.Infra.Context;
using Microsoft.EntityFrameworkCore;

namespace CurrencyApp.Infra.Repositories;

public class CurrencyRepository : ICurrencyRepository
{
    private readonly AppDbContext _context;

    public CurrencyRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Currency?> GetByIdAsync(int id)
    {
        return await _context.Currencies
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Currency?> GetByCodeAsync(string code)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();

        return await _context.Currencies
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Code == normalizedCode);
    }

    public async Task<IReadOnlyList<Currency>> GetPageAsync(int? cursor, int limit)
    {
        IQueryable<Currency> query = _context.Currencies
            .AsNoTracking()
            .OrderBy(x => x.Id);

        if (cursor.HasValue)
            query = query.Where(x => x.Id > cursor.Value);

        return await query
            .Take(limit)
            .ToListAsync();
    }

    public async Task AddAsync(Currency currency)
    {
        await _context.Currencies.AddAsync(currency);
    }

    public void Delete(Currency currency)
    {
        _context.Currencies.Remove(currency);
    }
}
