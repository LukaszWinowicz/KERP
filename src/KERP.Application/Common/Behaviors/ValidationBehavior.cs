using KERP.Application.Common.Abstractions.CQRS;
using KERP.Application.Common.Models;
using KERP.Application.Validation;

namespace KERP.Application.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : ICommandPipelineBehavior<TRequest, TResponse>
    where TResponse : Result
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Inicjalizuje nową instancję ValidationBehavior.
    /// </summary>
    /// <param name="validators">
    /// Kolekcja wszystkich walidatorów dla typu TRequest.
    /// DI automatycznie wstrzyknie wszystkie zarejestrowane walidatory.
    /// Jeśli nie ma walidatorów, kolekcja będzie pusta (nie null).
    /// </param>
    /// <param name="serviceProvider">
    /// Dostawca usług używany przez walidatory do rozwiązywania zależności.
    /// Walidatory mogą potrzebować dostępu do UserManager, Repositories itp.
    /// </param>
    public ValidationBehavior(
        IEnumerable<IValidator<TRequest>> validators,
        IServiceProvider serviceProvider) // ← NOWY PARAMETR
    {
        _validators = validators ?? throw new ArgumentNullException(nameof(validators));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
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

        if (!_validators.Any())
        {
            return await next();
        }

        // ═══════════════════════════════════════════════════════════════════════════════
        // KROK 2: Uruchom wszystkie walidatory równolegle
        // ═══════════════════════════════════════════════════════════════════════════════

        // Task.WhenAll wykonuje wszystkie walidatory jednocześnie
        // UWAGA: Teraz przekazujemy IServiceProvider do każdego walidatora!
        var validationTasks = _validators
            .Select(validator => validator.ValidateAsync(
                request,
                _serviceProvider, // ← PRZEKAZUJEMY ServiceProvider
                cancellationToken));

        var validationResults = await Task.WhenAll(validationTasks);

        // ═══════════════════════════════════════════════════════════════════════════════
        // KROK 3: Zbierz wszystkie błędy z wszystkich walidatorów
        // ═══════════════════════════════════════════════════════════════════════════════

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
            return ResultFactory.CreateFailure<TResponse>(resultErrors);
        }

        // ═══════════════════════════════════════════════════════════════════════════════
        // KROK 5: Walidacja OK - wywołaj następny behavior/handler
        // ═══════════════════════════════════════════════════════════════════════════════

        return await next();
    }
}