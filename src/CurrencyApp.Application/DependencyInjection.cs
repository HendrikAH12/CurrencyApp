using Microsoft.Extensions.DependencyInjection;
using CurrencyApp.Application.Services;
using CurrencyApp.Application.Contracts;

namespace CurrencyApp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICurrencyService, CurrencyService>();
        services.AddScoped<IExchangeRateService, ExchangeRateService>();
        
        return services;
    }
}
