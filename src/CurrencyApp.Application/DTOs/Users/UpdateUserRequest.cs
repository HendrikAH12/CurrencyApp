using System.ComponentModel.DataAnnotations;

namespace CurrencyApp.Application.DTOs.Users;

public sealed class UpdateUserRequest
{
    [MaxLength(120)]
    public string? Name { get; init; }

    [Range(1, int.MaxValue)]
    public int? MainCurrencyId { get; init; }

    public bool ClearMainCurrency { get; init; }
}
