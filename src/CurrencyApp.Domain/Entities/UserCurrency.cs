namespace CurrencyApp.Domain.Entities;

public class UserCurrency
{
    public int UserId { get; private set; }
    public int CurrencyId { get; private set; }
    public Currency Currency { get; private set; } = null!;
    public decimal Amount { get; private set; }

    private UserCurrency() { }

    public UserCurrency(Currency currency, decimal amount = 0)
    {
        if (currency is null)
            throw new ArgumentNullException(nameof(currency));

        Currency = currency;
        CurrencyId = currency.Id;

        SetAmount(amount);
    }

    public void SetAmount(decimal amount) {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative.");

        Amount = amount;
    }
}
