using System.ComponentModel.DataAnnotations;

namespace CurrencyApp.Application.DTOs.UserCurrencies;

public sealed class CreateUserCurrencyRequest
{
    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    public decimal Amount { get; init; }
}
