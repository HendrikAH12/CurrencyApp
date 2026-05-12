using CurrencyApp.Application.Common.Results;
using CurrencyApp.Application.DTOs.Common;
using CurrencyApp.Application.DTOs.UserCurrencies;
using CurrencyApp.Application.DTOs.Users;

namespace CurrencyApp.Application.Contracts;

public interface IUserService
{
    Task<Result<UserResponse>> GetByIdAsync(int id);

    Task<Result<CursorPagedResponse<UserResponse>>> GetAllAsync(string? cursor = null);

    Task<Result<UserResponse>> CreateAsync(CreateUserRequest request, CancellationToken ct = default);

    Task<Result<UserResponse>> UpdateAsync(int id, UpdateUserRequest request, CancellationToken ct = default);

    Task<Result<UserResponse>> AddOrUpdateCurrencyAsync(int id, int currencyId, CreateUserCurrencyRequest request, CancellationToken ct = default);

    Task<Result<UserResponse>> RemoveCurrency(int id, int currencyId, CancellationToken ct = default);

    Task<Result<Unit>> DeleteAsync(int id, CancellationToken ct = default);
}
