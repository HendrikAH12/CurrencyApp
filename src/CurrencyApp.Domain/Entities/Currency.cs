using CurrencyApp.Domain.Common;

namespace CurrencyApp.Domain.Entities;

public class Currency
{
    private const int MaxNameLength = 120;

    public int Id { get; private set; }
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;

    private Currency() { }

    public Currency(string code, string name)
    {
        SetCode(code);
        SetName(name);
    }

    public void SetCode(string code) => Code = CurrencyCodeRules.Normalize(code);

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.");

        var trimmedName = name.Trim();
        if (trimmedName.Length > MaxNameLength)
            throw new ArgumentException($"Name cannot exceed {MaxNameLength} characters.");

        Name = trimmedName;
    }
}
