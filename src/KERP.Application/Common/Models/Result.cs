using System.Reflection;

namespace KERP.Application.Common.Models;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public IReadOnlyCollection<Error> Errors { get; }

    protected Result(bool isSuccess, IReadOnlyCollection<Error> errors)
    {
        IsSuccess = isSuccess;
        Errors = errors ?? new List<Error>();
    }

    public static Result Success(List<RowValidationResult> results) => new Result(true, Array.Empty<Error>());

    public static Result Failure(IReadOnlyCollection<Error> errors)
    {
        if (errors == null || !errors.Any())
            throw new ArgumentException("Lista błędów nie może być pusta", nameof(errors));
        return new Result(false, errors);
    }

    public static TResult CreateFailure<TResult>(IReadOnlyCollection<Error> errors) where TResult : Result
    {
        if (errors == null || !errors.Any())
            throw new ArgumentException("Lista błędów nie może być pusta", nameof(errors));

        if (typeof(TResult) == typeof(Result))
        {
            return (Result.Failure(errors) as TResult)!;
        }

        if (typeof(TResult).IsGenericType && typeof(TResult).GetGenericTypeDefinition() == typeof(Result<>))
        {
            var genericArg = typeof(TResult).GetGenericArguments()[0];
            var constructed = typeof(Result<>).MakeGenericType(genericArg);
            var method = constructed.GetMethod("Failure", BindingFlags.Public | BindingFlags.Static);
            var result = method!.Invoke(null, new object[] { errors });
            return (TResult)result!;
        }

        throw new InvalidOperationException("Unknown result type");
    }
}

public class Result<TValue> : Result
{
    private readonly TValue _value;
    public TValue Value => IsSuccess ? _value : throw new InvalidOperationException("Nie można uzyskać wartości z rezultatu błędu.");

    public Result(TValue value)
        : base(true, Array.Empty<Error>())
    {
        _value = value;
    }

    protected Result(IReadOnlyCollection<Error> errors) : base(false, errors)
    {
    }

    public static Result<TValue> Success(TValue value) => new Result<TValue>(value);

    public static Result<TValue> Failure(IReadOnlyCollection<Error> errors)
    {
        if (errors == null || !errors.Any())
            throw new ArgumentException("Lista błędów nie może być pusta", nameof(errors));
        return new Result<TValue>(errors);
    }
}