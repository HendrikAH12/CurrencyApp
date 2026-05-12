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

    public void SetCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code cannot be empty.");

        code = code.Trim().ToUpperInvariant();
        if (code.Length != 3 || !code.All(char.IsLetter))
            throw new ArgumentException("Code must be a 3-letter ISO currency code.");

        Code = code;
    }

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
