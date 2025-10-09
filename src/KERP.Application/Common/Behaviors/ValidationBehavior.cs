using KERP.Application.Common.Abstractions;
using KERP.Application.Common.Models;
using KERP.Application.Validation;

namespace KERP.Application.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : ICommandPipelineBehavior<TRequest, TResponse>
    where TResponse : Result
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    /// <summary>
    /// Inicjalizuje nową instancję ValidationBehavior.
    /// </summary>
    /// <param name="validators">
    /// Kolekcja wszystkich walidatorów dla typu TRequest.
    /// DI automatycznie wstrzyknie wszystkie zarejestrowane walidatory.
    /// Jeśli nie ma walidatorów, kolekcja będzie pusta (nie null).
    /// </param>
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators ?? throw new ArgumentNullException(nameof(validators));
    }

    /// <inheritdoc />
    public async Task<TResponse> HandleAsync(
        TRequest request,
        Func<Task<TResponse>> next,
        CancellationToken cancellationToken = default)
    {
        // ═══════════════════════════════════════════════════════════════════════════════
        // KROK 1: Sprawdź czy są jakieś walidatory zarejestrowane
        // ═══════════════════════════════════════════════════════════════════════════════

        // Jeśli nie ma walidatorów, od razu wywołaj next()
        // (oszczędność - nie wykonujemy pustej pętli)
        if (!_validators.Any())
        {
            return await next();
        }

        // ═══════════════════════════════════════════════════════════════════════════════
        // KROK 2: Uruchom wszystkie walidatory równolegle
        // ═══════════════════════════════════════════════════════════════════════════════

        // Task.WhenAll wykonuje wszystkie walidatory jednocześnie
        // To jest szybsze niż wykonywanie ich po kolei
        var validationTasks = _validators
            .Select(validator => validator.ValidateAsync(request, cancellationToken));

        var validationResults = await Task.WhenAll(validationTasks);

        // ═══════════════════════════════════════════════════════════════════════════════
        // KROK 3: Zbierz wszystkie błędy z wszystkich walidatorów
        // ═══════════════════════════════════════════════════════════════════════════════

        // SelectMany "spłaszcza" listy błędów z wielu walidatorów w jedną listę
        // Przykład:
        // Validator1.Errors = [Error1, Error2]
        // Validator2.Errors = [Error3]
        // SelectMany → [Error1, Error2, Error3]
        var errors = validationResults
            .SelectMany(result => result.Errors)
            .ToList();

        // ═══════════════════════════════════════════════════════════════════════════════
        // KROK 4: Jeśli są błędy, PRZERWIJ PIPELINE i zwróć Result.Failure
        // ═══════════════════════════════════════════════════════════════════════════════

        if (errors.Any())
        {
            // Konwertuj ValidationError (z walidatorów) na Error (z Result pattern)
            var resultErrors = errors
                .Select(validationError => new Error(
                    Code: "ValidationError",
                    Description: validationError.ErrorMessage,
                    Type: ErrorType.Critical))
                .ToList();

            // ⚠️ WAŻNE: NIE wywołujemy next() - pipeline jest przerwany!
            // Handler się nie wykona, transakcja nie zostanie rozpoczęta.
            return CreateFailureResult(resultErrors);
        }

        // ═══════════════════════════════════════════════════════════════════════════════
        // KROK 5: Walidacja OK - wywołaj następny behavior/handler
        // ═══════════════════════════════════════════════════════════════════════════════

        return await next();
    }

    /// <summary>
    /// Tworzy Result.Failure dla typu TResponse.
    /// </summary>
    /// <remarks>
    /// Ta metoda jest identyczna jak w ExceptionHandlingBehavior.
    /// W idealnym świecie, byłaby w osobnej klasie pomocniczej (np. ResultFactory).
    /// </remarks>
    private static TResponse CreateFailureResult(IReadOnlyCollection<Error> errors)
    {
        var responseType = typeof(TResponse);

        if (responseType == typeof(Result))
        {
            var result = Result.Failure(errors);
            return (TResponse)(object)result;
        }

        if (responseType.IsGenericType &&
            responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var valueType = responseType.GetGenericArguments()[0];
            var resultType = typeof(Result<>).MakeGenericType(valueType);
            var failureMethod = resultType.GetMethod(
                "Failure",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static,
                null,
                new[] { typeof(IReadOnlyCollection<Error>) },
                null);

            if (failureMethod == null)
            {
                throw new InvalidOperationException(
                    $"Could not find Failure method on type {resultType.Name}.");
            }

            var result = failureMethod.Invoke(null, new object[] { errors });

            if (result == null)
            {
                throw new InvalidOperationException(
                    $"Failure method on {resultType.Name} returned null.");
            }

            return (TResponse)result;
        }

        throw new InvalidOperationException(
            $"Unsupported result type: {responseType.Name}.");
    }

}