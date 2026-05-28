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
            if (!response.IsSuccessStatusCode)
                return (false, 0);

            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            using var json = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

            if (!json.RootElement.TryGetProperty("rates", out var rates))
                return (false, 0);

            if (!rates.TryGetProperty(toCode, out var rateElement))
                return (false, 0);

            if (!rateElement.TryGetDecimal(out var rate))
                return (false, 0);

            return (true, rate);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            return (false, 0);
        }
    }
}
