using CurrencyApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CurrencyApp.Infra.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserCurrency> UserCurrencies => Set<UserCurrency>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
