using CurrencyApp.Application.Common.Results;
using CurrencyApp.Application.Contracts;
using CurrencyApp.Application.DTOs.Currencies;
using CurrencyApp.Api.Common;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyApp.Api.Controllers;

[ApiController]
[Route("currencies")]
public class CurrenciesController : ControllerBase
{
    private readonly ICurrencyService _currencyService;

    public CurrenciesController(ICurrencyService currencyService)
    {
        _currencyService = currencyService;
    }

    [HttpGet("{id:int}", Name = "GetCurrencyById")]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var result = await _currencyService.GetByIdAsync(id);

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
        var result = await _currencyService.GetAllAsync(cursor);

        return result.Type switch
        {
            ResultType.Success => Ok(ApiResponseFactory.PaginatedData(result.Data!.Items, result.Data.NextCursor)),
            ResultType.Invalid => BadRequest(ApiResponseFactory.Error(result.Error)),
            _ => StatusCode(StatusCodes.Status500InternalServerError, ApiResponseFactory.Error($"Result type '{result.Type}' is not handled."))
        };
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateCurrencyRequest request, CancellationToken ct = default)
    {
        var result = await _currencyService.CreateAsync(request, ct);

        return result.Type switch
        {
            ResultType.Created => CreatedAtRoute("GetCurrencyById", new { id = result.Data!.Id }, ApiResponseFactory.Data(result.Data)),
            ResultType.Conflict => Conflict(ApiResponseFactory.Error(result.Error)),
            ResultType.Invalid => BadRequest(ApiResponseFactory.Error(result.Error)),
            _ => StatusCode(StatusCodes.Status500InternalServerError, ApiResponseFactory.Error($"Result type '{result.Type}' is not handled."))
        };
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken ct = default)
    {
        var result = await _currencyService.DeleteAsync(id, ct);

        return result.Type switch
        {
            ResultType.Success => NoContent(),
            ResultType.NotFound => NotFound(ApiResponseFactory.Error(result.Error)),
            ResultType.Invalid => BadRequest(ApiResponseFactory.Error(result.Error)),
            _ => StatusCode(StatusCodes.Status500InternalServerError, ApiResponseFactory.Error($"Result type '{result.Type}' is not handled."))
        };
    }
}
