using CurrencyApp.Application.DTOs.Currencies;
using CurrencyApp.Application.Common.Results;
using CurrencyApp.Application.DTOs.Common;

namespace CurrencyApp.Application.Contracts;

public interface ICurrencyService
{
   Task<Result<CurrencyResponse>> GetByIdAsync(int id);

    Task<Result<CursorPagedResponse<CurrencyResponse>>> GetAllAsync(string? cursor = null);

    Task<Result<CurrencyResponse>> CreateAsync(CreateCurrencyRequest request, CancellationToken ct = default);

    Task<Result<Unit>> DeleteAsync(int id, CancellationToken ct = default);
}
