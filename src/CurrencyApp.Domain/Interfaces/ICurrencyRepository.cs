using CurrencyApp.Domain.Entities;

namespace CurrencyApp.Domain.Interfaces;

public interface ICurrencyRepository
{
    Task<Currency?> GetByIdAsync(int id);

    Task<Currency?> GetByCodeAsync(string code);

    Task<IReadOnlyList<Currency>> GetPageAsync(int? cursor, int limit);

    Task AddAsync(Currency currency);

    void Delete(Currency currency);
}
