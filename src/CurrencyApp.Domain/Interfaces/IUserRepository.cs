using CurrencyApp.Domain.Entities;

namespace CurrencyApp.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);

    Task<User?> GetByEmailAsync(string email);

    Task<IReadOnlyList<User>> GetPageAsync(int? cursor, int limit);

    Task AddAsync(User user);

    void Update(User user);

    void Delete(User user);
}
