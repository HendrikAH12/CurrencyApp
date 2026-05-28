namespace CurrencyApp.Domain.Common;

public static class CurrencyCodeRules
{
    public const int Length = 3;

    public static string Normalize(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Currency code cannot be empty.", nameof(code));

        var normalized = code.Trim().ToUpperInvariant();
        if (normalized.Length != Length || !normalized.All(char.IsLetter))
            throw new ArgumentException("Currency code must be a 3-letter ISO code.", nameof(code));

        return normalized;
    }
}
