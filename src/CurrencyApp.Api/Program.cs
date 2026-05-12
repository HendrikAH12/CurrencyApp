using CurrencyApp.Application;
using CurrencyApp.Api.Middleware;
using CurrencyApp.Infra;

namespace CurrencyApp.Api;

public class Program
{
    private const string DefaultSqliteConnection = "Data Source=currencyapp.db";

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
            ?? DefaultSqliteConnection;

        builder.Services.AddInfrastructure(connectionString);
        builder.Services.AddApplication();
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new()
            {
                Title = "CurrencyApp API",
                Version = "v1",
                Description = "API for managing users, currencies and user holdings."
            });
        });

        var app = builder.Build();

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "CurrencyApp API v1");
            options.RoutePrefix = "swagger";
        });

        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.MapControllers();

        app.Run();
    }
}
