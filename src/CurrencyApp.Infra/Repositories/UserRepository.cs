using CurrencyApp.Domain.Entities;
using CurrencyApp.Domain.Interfaces;
using CurrencyApp.Infra.Context;
using Microsoft.EntityFrameworkCore;

namespace CurrencyApp.Infra.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users
            .Include(x => x.MainCurrency)
            .Include(x => x.Holdings)
                .ThenInclude(x => x.Currency)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        return await _context.Users
            .Include(x => x.MainCurrency)
            .Include(x => x.Holdings)
                .ThenInclude(x => x.Currency)
            .FirstOrDefaultAsync(x => x.Email == normalizedEmail);
    }

    public async Task<IReadOnlyList<User>> GetPageAsync(int? cursor, int limit)
    {
        IQueryable<User> query = _context.Users
            .Include(x => x.MainCurrency)
            .Include(x => x.Holdings)
                .ThenInclude(x => x.Currency)
            .AsNoTracking()
            .OrderBy(x => x.Id);

        if (cursor.HasValue)
            query = query.Where(x => x.Id > cursor.Value);

        return await query
            .Take(limit)
            .ToListAsync();
    }

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
    }

    public void Update(User user)
    {
        _context.Users.Update(user);
    }

    public void Delete(User user)
    {
        _context.Users.Remove(user);
    }
}
