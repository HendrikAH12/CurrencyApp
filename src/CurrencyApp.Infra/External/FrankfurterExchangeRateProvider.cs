using System.Text.Json;
using CurrencyApp.Application.Contracts;

namespace CurrencyApp.Infra.External;

public class FrankfurterExchangeRateProvider : IExchangeRateProvider
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://api.frankfurter.app";

    public FrankfurterExchangeRateProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<(bool Success, decimal Rate)> FetchRateAsync(string fromCode, string toCode, CancellationToken ct = default)
    {
        try
        {
            var requestUrl = $"{BaseUrl}/latest?from={fromCode}&to={toCode}";

            using var response = await _httpClient.GetAsync(requestUrl, ct);
            if (response.IsSuccessStatusCode)
            {
                await using var stream = await response.Content.ReadAsStreamAsync(ct);
                using var json = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

                if (json.RootElement.TryGetProperty("rates", out var rates)
                    && rates.TryGetProperty(toCode, out var rateElement)
                    && rateElement.TryGetDecimal(out var rate))
                {
                    return (true, rate);
                }
            }
        }
        catch (Exception)
        {
        }

        return (false, 0);
    }
}
