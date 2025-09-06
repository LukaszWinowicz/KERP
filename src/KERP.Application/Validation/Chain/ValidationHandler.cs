namespace KERP.Application.Validation.Chain;

/// <summary>
/// Generyczna klasa bazowa dla każdeo ogniwa w łańuchu walidacji.
/// </summary>
/// <typeparam name="T">Typ obiektu poddawanego walidacji.</typeparam>
public abstract class ValidationHandler<T>
{
    protected ValidationHandler<T>? _nextHandler;
    public ValidationHandler<T> SetNext(ValidationHandler<T> nextHandler)
    {
        _nextHandler = nextHandler;
        return _nextHandler;
    }

    /// <summary>
    /// Uruchamia walidację. Może być asynchroniczna, aby obsługiwać zapytania do bazy danych.
    /// </summary>
    public async Task HandleAsync(ValidationContext<T> context)
    {
        if (context.ShouldStop)
        {
            return;
        }

        await ValidateAsync(context);

        if (_nextHandler is not null && !context.ShouldStop)
        {
            await _nextHandler.HandleAsync(context);
        }
    }

    protected abstract Task ValidateAsync(ValidationContext<T> context);
}