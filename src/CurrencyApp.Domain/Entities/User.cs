using System.Net.Mail;

namespace CurrencyApp.Domain.Entities;

public class User
{
    private const int MaxNameLength = 120;
    private const int MaxEmailLength = 254;

    public int Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public int? MainCurrencyId { get; private set; }
    public Currency? MainCurrency { get; private set; }
    private readonly List<UserCurrency> _holdings;
    public IReadOnlyCollection<UserCurrency> Holdings => _holdings.AsReadOnly();

    public User()
    {
        _holdings = new List<UserCurrency>();
    }

    public User(string name, string email) : this()
    {
        SetName(name);
        SetEmail(email);
    }

    public void SetName(string name) {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.");

        var trimmedName = name.Trim();
        if (trimmedName.Length > MaxNameLength)
            throw new ArgumentException($"Name cannot exceed {MaxNameLength} characters.");

        Name = trimmedName;
    }

    public void SetEmail(string email) {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty.");

        try
        {
            _ = new MailAddress(email);
        }
        catch
        {
            throw new ArgumentException($"Invalid email: {email}");
        }

        var normalizedEmail = email.Trim().ToLowerInvariant();
        if (normalizedEmail.Length > MaxEmailLength)
            throw new ArgumentException($"Email cannot exceed {MaxEmailLength} characters.");

        Email = normalizedEmail;
    }

    public void SetMainCurrency(Currency currency)
    {
        if (currency == null)
            throw new ArgumentNullException(nameof(currency));

        if (!_holdings.Any(h => h.CurrencyId == currency.Id))
            throw new InvalidOperationException($"User does not have currency {currency.Code} in holdings.");

        MainCurrencyId = currency.Id;
        MainCurrency = currency;
    }

    public void ClearMainCurrency()
    {
        MainCurrencyId = null;
        MainCurrency = null;
    }

    public void AddOrUpdateCurrency(Currency currency, decimal amount)
    {
        if (currency == null)
            throw new ArgumentNullException(nameof(currency));

        var existing = _holdings.FirstOrDefault(h => h.CurrencyId == currency.Id);

        if (existing != null)
        {
            existing.SetAmount(amount);
            return;
        }

        _holdings.Add(new UserCurrency(currency, amount));
    }

    public void RemoveCurrency(int currencyId)
    {
        var existing = _holdings.FirstOrDefault(h => h.CurrencyId == currencyId);
        if (existing == null)
            return;

        if (MainCurrencyId == currencyId)
            ClearMainCurrency();

        _holdings.Remove(existing);
    }
}
