using CurrencyApp.Domain.Common;

namespace CurrencyApp.Domain.Entities;

public class ExchangeRateCache
{
    public string FromCode { get; private set; } = null!;
    public string ToCode { get; private set; } = null!;
    public decimal Rate { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }

    private ExchangeRateCache() { }

    public ExchangeRateCache(string fromCode, string toCode, decimal rate, DateTime expiresAtUtc)
    {
        SetFromCode(fromCode);
        SetToCode(toCode);
        SetRate(rate);
        SetExpiresAtUtc(expiresAtUtc);
    }

    public void Refresh(decimal rate, DateTime expiresAtUtc)
    {
        SetRate(rate);
        SetExpiresAtUtc(expiresAtUtc);
    }

    public void SetFromCode(string fromCode) => FromCode = CurrencyCodeRules.Normalize(fromCode);

    public void SetToCode(string toCode) => ToCode = CurrencyCodeRules.Normalize(toCode);

    public void SetRate(decimal rate)
    {
        if (rate <= 0)
            throw new ArgumentException("Rate must be greater than zero.");

        Rate = rate;
    }

    public void SetExpiresAtUtc(DateTime expiresAtUtc)
    {
        if (expiresAtUtc.Kind == DateTimeKind.Unspecified)
            throw new ArgumentException("ExpiresAtUtc must be UTC.", nameof(expiresAtUtc));

        ExpiresAtUtc = expiresAtUtc.Kind == DateTimeKind.Utc
            ? expiresAtUtc
            : expiresAtUtc.ToUniversalTime();
    }
}
