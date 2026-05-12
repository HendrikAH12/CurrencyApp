using CurrencyApp.Domain.Interfaces;
using CurrencyApp.Infra.Context;
using CurrencyApp.Infra.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CurrencyApp.Infra;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<ICurrencyRepository, CurrencyRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
