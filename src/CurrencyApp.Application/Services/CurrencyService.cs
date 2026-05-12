using CurrencyApp.Application.Contracts;
using CurrencyApp.Application.Common.Pagination;
using CurrencyApp.Application.DTOs.Common;
using CurrencyApp.Application.DTOs.Currencies;
using CurrencyApp.Domain.Entities;
using CurrencyApp.Domain.Interfaces;
using CurrencyApp.Application.Common.Results;

namespace CurrencyApp.Application.Services;

public class CurrencyService : ICurrencyService
{
    private const int PageSize = 10;
    private readonly ICurrencyRepository _currencyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CurrencyService(ICurrencyRepository currencyRepository, IUnitOfWork unitOfWork)
    {
        _currencyRepository = currencyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CurrencyResponse>> GetByIdAsync(int id)
    {
        var currency = await _currencyRepository.GetByIdAsync(id);

        if (currency is null)
            return Result<CurrencyResponse>.NotFound("Currency not found.");

        return Result<CurrencyResponse>.Success(new CurrencyResponse
        {
            Id = currency.Id,
            Code = currency.Code,
            Name = currency.Name
        });
    }

    public async Task<Result<CursorPagedResponse<CurrencyResponse>>> GetAllAsync(string? cursor = null)
    {
        int? decodedCursor = null;

        if (!string.IsNullOrWhiteSpace(cursor))
        {
            if (!CursorTokenCodec.TryDecode(cursor, out var id))
                return Result<CursorPagedResponse<CurrencyResponse>>.Invalid("Invalid cursor token.");

            decodedCursor = id;
        }

        var currencies = await _currencyRepository.GetPageAsync(decodedCursor, PageSize + 1);
        var hasMore = currencies.Count > PageSize;
        var currentPage = hasMore ? currencies.Take(PageSize).ToList() : currencies.ToList();

        var result = currentPage.Select(c => new CurrencyResponse
        {
            Id = c.Id,
            Code = c.Code,
            Name = c.Name
        })
        .ToList();
        
        string? nextCursor = hasMore ? CursorTokenCodec.Encode(currentPage[^1].Id) : null;

        return Result<CursorPagedResponse<CurrencyResponse>>.Success(new CursorPagedResponse<CurrencyResponse>
        {
            Items = result,
            NextCursor = nextCursor
        });
    }

    public async Task<Result<CurrencyResponse>> CreateAsync(CreateCurrencyRequest request, CancellationToken ct = default)
    {
        var currency = new Currency(request.Code, request.Name);

        var existingCurrency = await _currencyRepository.GetByCodeAsync(currency.Code);
        if (existingCurrency is not null)
            return Result<CurrencyResponse>.Conflict($"Currency code '{currency.Code}' already exists.");

        await _currencyRepository.AddAsync(currency);
        await _unitOfWork.CommitAsync(ct);

        return Result<CurrencyResponse>.Created(new CurrencyResponse
        {
            Id = currency.Id,
            Code = currency.Code,
            Name = currency.Name
        });
    }

    public async Task<Result<Unit>> DeleteAsync(int id, CancellationToken ct = default)
    {
        var currency = await _currencyRepository.GetByIdAsync(id);

        if (currency is null)
            return Result<Unit>.NotFound("Currency not found.");

        _currencyRepository.Delete(currency);
        await _unitOfWork.CommitAsync(ct);

        return Result<Unit>.Success(Unit.Value);
    }
}
