using CurrencyApp.Domain.Interfaces;

namespace CurrencyApp.Infra.Context;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public async Task CommitAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }
}
