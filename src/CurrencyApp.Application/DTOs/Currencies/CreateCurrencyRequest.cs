using System.ComponentModel.DataAnnotations;

namespace CurrencyApp.Application.DTOs.Currencies;

public sealed class CreateCurrencyRequest
{
    [Required]
    [RegularExpression("^[A-Za-z]{3}$")]
    public required string Code { get; init; }

    [Required]
    [MaxLength(120)]
    public required string Name { get; init; }
}
