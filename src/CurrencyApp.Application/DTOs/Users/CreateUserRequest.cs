using System.ComponentModel.DataAnnotations;

namespace CurrencyApp.Application.DTOs.Users;

public sealed class CreateUserRequest
{
    [Required]
    [MaxLength(120)]
    public required string Name { get; init; }

    [Required]
    [EmailAddress]
    [MaxLength(254)]
    public required string Email { get; init; }
}
