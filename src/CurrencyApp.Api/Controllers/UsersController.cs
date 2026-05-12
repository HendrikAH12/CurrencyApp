using CurrencyApp.Application.Common.Results;
using CurrencyApp.Application.Contracts;
using CurrencyApp.Application.DTOs.UserCurrencies;
using CurrencyApp.Application.DTOs.Users;
using CurrencyApp.Api.Common;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyApp.Api.Controllers;

[ApiController]
[Route("users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("{id:int}", Name = "GetUserById")]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var result = await _userService.GetByIdAsync(id);

        return result.Type switch
        {
            ResultType.Success => Ok(ApiResponseFactory.Data(result.Data)),
            ResultType.NotFound => NotFound(ApiResponseFactory.Error(result.Error)),
            ResultType.Invalid => BadRequest(ApiResponseFactory.Error(result.Error)),
            _ => StatusCode(StatusCodes.Status500InternalServerError, ApiResponseFactory.Error($"Result type '{result.Type}' is not handled."))
        };
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAsync([FromQuery] string? cursor = null)
    {
        var result = await _userService.GetAllAsync(cursor);

        return result.Type switch
        {
            ResultType.Success => Ok(ApiResponseFactory.PaginatedData(result.Data!.Items, result.Data.NextCursor)),
            ResultType.Invalid => BadRequest(ApiResponseFactory.Error(result.Error)),
            _ => StatusCode(StatusCodes.Status500InternalServerError, ApiResponseFactory.Error($"Result type '{result.Type}' is not handled."))
        };
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateUserRequest request, CancellationToken ct = default)
    {
        var result = await _userService.CreateAsync(request, ct);

        return result.Type switch
        {
            ResultType.Created => CreatedAtRoute("GetUserById", new { id = result.Data!.Id }, ApiResponseFactory.Data(result.Data)),
            ResultType.Conflict => Conflict(ApiResponseFactory.Error(result.Error)),
            ResultType.Invalid => BadRequest(ApiResponseFactory.Error(result.Error)),
            _ => StatusCode(StatusCodes.Status500InternalServerError, ApiResponseFactory.Error($"Result type '{result.Type}' is not handled."))
        };
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateAsync(int id, UpdateUserRequest request, CancellationToken ct = default)
    {
        var result = await _userService.UpdateAsync(id, request, ct);

        return result.Type switch
        {
            ResultType.Success => Ok(ApiResponseFactory.Data(result.Data)),
            ResultType.NotFound => NotFound(ApiResponseFactory.Error(result.Error)),
            ResultType.Invalid => BadRequest(ApiResponseFactory.Error(result.Error)),
            _ => StatusCode(StatusCodes.Status500InternalServerError, ApiResponseFactory.Error($"Result type '{result.Type}' is not handled."))
        };
    }

    [HttpPost("{id:int}/currencies/{currencyId:int}")]
    public async Task<IActionResult> AddOrUpdateCurrencyAsync(int id, int currencyId, [FromBody] CreateUserCurrencyRequest request, CancellationToken ct = default)
    {
        var result = await _userService.AddOrUpdateCurrencyAsync(id, currencyId, request, ct);

        return result.Type switch
        {
            ResultType.Success => Ok(ApiResponseFactory.Data(result.Data)),
            ResultType.NotFound => NotFound(ApiResponseFactory.Error(result.Error)),
            ResultType.Invalid => BadRequest(ApiResponseFactory.Error(result.Error)),
            _ => StatusCode(StatusCodes.Status500InternalServerError, ApiResponseFactory.Error($"Result type '{result.Type}' is not handled."))
        };
    }

    [HttpDelete("{id:int}/currencies/{currencyId:int}")]
    public async Task<IActionResult> RemoveCurrencyAsync(int id, int currencyId, CancellationToken ct = default)
    {
        var result = await _userService.RemoveCurrency(id, currencyId, ct);

        return result.Type switch
        {
            ResultType.Success => NoContent(),
            ResultType.NotFound => NotFound(ApiResponseFactory.Error(result.Error)),
            ResultType.Invalid => BadRequest(ApiResponseFactory.Error(result.Error)),
            _ => StatusCode(StatusCodes.Status500InternalServerError, ApiResponseFactory.Error($"Result type '{result.Type}' is not handled."))
        };
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken ct = default)
    {
        var result = await _userService.DeleteAsync(id, ct);

        return result.Type switch
        {
            ResultType.Success => NoContent(),
            ResultType.NotFound => NotFound(ApiResponseFactory.Error(result.Error)),
            ResultType.Invalid => BadRequest(ApiResponseFactory.Error(result.Error)),
            _ => StatusCode(StatusCodes.Status500InternalServerError, ApiResponseFactory.Error($"Result type '{result.Type}' is not handled."))
        };
    }
}
