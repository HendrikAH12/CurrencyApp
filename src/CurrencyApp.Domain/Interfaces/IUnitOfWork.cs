namespace CurrencyApp.Domain.Interfaces;

public interface IUnitOfWork
{
    Task CommitAsync(CancellationToken ct = default);
}
