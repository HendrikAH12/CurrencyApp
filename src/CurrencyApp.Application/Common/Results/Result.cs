namespace CurrencyApp.Application.Common.Results;

public class Result<T>
{
    public bool IsSuccess { get; private set; }
    public ResultType Type { get; private set; }
    public T? Data { get; private set; }
    public string? Error { get; private set; }

    private Result() { }

    public static Result<T> Success(T data)
        => new() { IsSuccess = true, Type = ResultType.Success, Data = data };

    public static Result<T> Created(T data)
        => new() { IsSuccess = true, Type = ResultType.Created, Data = data };

    public static Result<T> NotFound(string error)
        => new() { IsSuccess = false, Type = ResultType.NotFound, Error = error };

    public static Result<T> Conflict(string error)
        => new() { IsSuccess = false, Type = ResultType.Conflict, Error = error };

    public static Result<T> Invalid(string error)
        => new() { IsSuccess = false, Type = ResultType.Invalid, Error = error };
}
