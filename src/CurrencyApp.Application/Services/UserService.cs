using CurrencyApp.Application.Common.Results;
using CurrencyApp.Application.Common.Pagination;
using CurrencyApp.Application.Contracts;
using CurrencyApp.Application.DTOs.Common;
using CurrencyApp.Application.DTOs.UserCurrencies;
using CurrencyApp.Application.DTOs.Users;
using CurrencyApp.Domain.Entities;
using CurrencyApp.Domain.Interfaces;

namespace CurrencyApp.Application.Services;

public class UserService : IUserService
{
    private const int PageSize = 10;
    private readonly IUserRepository _userRepository;
    private readonly ICurrencyRepository _currencyRepository;
    private readonly IExchangeRateService _exchangeRateService;
    private readonly IUnitOfWork _unitOfWork;

    public UserService(IUserRepository userRepository, ICurrencyRepository currencyRepository, IExchangeRateService exchangeRateService, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _currencyRepository = currencyRepository;
        _exchangeRateService = exchangeRateService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UserResponse>> GetByIdAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);

        if (user is null)
            return Result<UserResponse>.NotFound("User not found.");

        var response = MapToResponse(user);

        if (user.MainCurrency is null)
            return Result<UserResponse>.Success(response);

        decimal totalInMainCurrency = 0;
        foreach (var holding in user.Holdings)
        {
            var rate = await _exchangeRateService.GetRateAsync(holding.Currency.Code, user.MainCurrency.Code);
            if (rate == 0)
            {
                response.TotalInMainCurrency = null;
                return Result<UserResponse>.Success(response);
            }

            totalInMainCurrency += holding.Amount * rate;
        }

        response.TotalInMainCurrency = Math.Round(totalInMainCurrency, 2);
        return Result<UserResponse>.Success(response);
    }

    public async Task<Result<CursorPagedResponse<UserResponse>>> GetAllAsync(string? cursor = null)
    {
        int? decodedCursor = null;

        if (!string.IsNullOrWhiteSpace(cursor))
        {
            if (!CursorTokenCodec.TryDecode(cursor, out var id))
                return Result<CursorPagedResponse<UserResponse>>.Invalid("Invalid cursor token.");

            decodedCursor = id;
        }

        var users = await _userRepository.GetPageAsync(decodedCursor, PageSize + 1);
        var hasMore = users.Count > PageSize;
        var currentPage = hasMore ? users.Take(PageSize).ToList() : users.ToList();

        var result = currentPage.Select(MapToResponse).ToList();
        
        string? nextCursor = hasMore ? CursorTokenCodec.Encode(currentPage[^1].Id) : null;

        return Result<CursorPagedResponse<UserResponse>>.Success(new CursorPagedResponse<UserResponse>
        {
            Items = result,
            NextCursor = nextCursor
        });
    }

    public async Task<Result<UserResponse>> CreateAsync(CreateUserRequest request, CancellationToken ct = default)
    {
        var user = new User(request.Name, request.Email);

        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser is not null)
            return Result<UserResponse>.Conflict("A user with this email already exists.");

        await _userRepository.AddAsync(user);
        await _unitOfWork.CommitAsync(ct);

        return Result<UserResponse>.Created(new UserResponse
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email
        });
    }

    public async Task<Result<UserResponse>> UpdateAsync(int id, UpdateUserRequest request, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(id);

        if (user is null)
            return Result<UserResponse>.NotFound("User not found.");

        if (request.Name is not null)
            user.SetName(request.Name);

        if (request.ClearMainCurrency && request.MainCurrencyId.HasValue)
            return Result<UserResponse>.Invalid("Cannot clear and set MainCurrencyId in the same request.");

        if (request.ClearMainCurrency)
            user.ClearMainCurrency();

        if (request.MainCurrencyId.HasValue)
        {
            var currency = await _currencyRepository.GetByIdAsync(request.MainCurrencyId!.Value);

            if (currency is null)
                return Result<UserResponse>.NotFound("Currency not found.");

            user.SetMainCurrency(currency);
        }

        _userRepository.Update(user);
        await _unitOfWork.CommitAsync(ct);

        return Result<UserResponse>.Success(MapToResponse(user));
    }

    public async Task<Result<UserResponse>> AddOrUpdateCurrencyAsync(int id, int currencyId, CreateUserCurrencyRequest request, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(id);

        if (user is null)
            return Result<UserResponse>.NotFound("User not found.");

        var currency = await _currencyRepository.GetByIdAsync(currencyId);

        if (currency is null)
            return Result<UserResponse>.NotFound("Currency not found.");

        user.AddOrUpdateCurrency(currency, request.Amount);

        _userRepository.Update(user);
        await _unitOfWork.CommitAsync(ct);

        return Result<UserResponse>.Success(MapToResponse(user));
    }

    public async Task<Result<UserResponse>> RemoveCurrency(int id, int currencyId, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(id);

        if (user is null)
            return Result<UserResponse>.NotFound("User not found.");

        user.RemoveCurrency(currencyId);
        _userRepository.Update(user);
        await _unitOfWork.CommitAsync(ct);

        return Result<UserResponse>.Success(MapToResponse(user));
    }

    public async Task<Result<Unit>> DeleteAsync(int id, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(id);

        if (user is null)
            return Result<Unit>.NotFound("User not found.");

        _userRepository.Delete(user);
        await _unitOfWork.CommitAsync(ct);

        return Result<Unit>.Success(Unit.Value);
    }

    private static UserResponse MapToResponse(User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            MainCurrencyId = user.MainCurrencyId,
            MainCurrencyCode = user.MainCurrency?.Code,
            Holdings = user.Holdings
                .Select(holding => new UserCurrencyResponse
                {
                    UserId = holding.UserId,
                    CurrencyId = holding.CurrencyId,
                    CurrencyCode = holding.Currency.Code,
                    Amount = holding.Amount
                })
                .ToList()
        };
    }
}
