using KERP.Application.Common.Abstractions.CQRS;
using KERP.Application.Common.Models;
using KERP.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace KERP.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior odpowiedzialny za przechwytywanie i obsługę wyjątków.
/// Konwertuje wyjątki na Result.Failure, zapewniając spójną obsługę błędów w całym systemie.
/// Implementuje zarówno ICommandPipelineBehavior jak i IQueryPipelineBehavior.
/// </summary>
/// <typeparam name="TRequest">Typ requestu (Command lub Query).</typeparam>
/// <typeparam name="TResponse">Typ odpowiedzi (Result lub Result&lt;T&gt;).</typeparam>
public class ExceptionHandlingBehavior<TRequest, TResponse>
    : ICommandPipelineBehavior<TRequest, TResponse>,
      IQueryPipelineBehavior<TRequest, TResponse>
    where TResponse : Result

{
    private readonly ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> _logger;

    /// <summary>
    /// Inicjalizuje nową instancję ExceptionHandlingBehavior.
    /// </summary>
    /// <param name="logger">Logger do zapisywania informacji o wyjątkach.</param>
    public ExceptionHandlingBehavior(ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<TResponse> HandleAsync(
        TRequest request,
        Func<Task<TResponse>> next,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Wywołaj następny behavior (lub handler, jeśli to ostatni behavior)
            return await next();
        }
        // CATCH 1: Specyficzny wyjątek biznesowy (przewidywalny)
        catch (BusinessRuleValidationException ex)
        {

            // To jest PRZEWIDYWALNY błąd - naruszenie reguły biznesowej
            // Przykłady:
            // - "Nie można usunąć produktu z zamówieniami"
            // - "Kwota zamówienia przekracza limit kredytowy"
            // - "Nie można zmienić statusu zamówienia na 'Wysłane' bez adresu"

            var requestName = typeof(TRequest).Name;

            // Loguj jako WARNING (nie ERROR!) - to nie jest awaria systemu
            _logger.LogWarning(
                "Business rule validation failed for request {RequestName}: {ErrorMessage}",
                requestName,
                ex.Message);

            // Komunikat z wyjątku jest BEZPIECZNY dla użytkownika
            // (developer napisał go świadomie jako feedback dla użytkownika)
            var error = new Error(
                Code: "BusinessRuleViolation",
                Description: ex.Message,
                Type: ErrorType.Critical);

            // Konwertuj na Result.Failure używając nowej fabryki
            return ResultFactory.CreateFailure<TResponse>(new[] { error });
        }

        // CATCH 2: Wszystkie inne wyjątki (nieoczekiwane)
        catch (Exception ex)
        {
            // To jest NIEOCZEKIWANY błąd - awaria systemu
            // Przykłady:
            // - SqlException (problem z bazą danych)
            // - NullReferenceException (bug w kodzie)
            // - OutOfMemoryException (problem z zasobami)
            // - HttpRequestException (problem z zewnętrznym API)

            var requestName = typeof(TRequest).Name;

            // Loguj jako ERROR - to jest problem, który wymaga uwagi developerów!
            _logger.LogError(
                ex,
                "An unhandled exception occurred while processing request {RequestName}",
                requestName);

            // Komunikat dla użytkownika jest GENERYCZNY
            // NIE ujawniamy szczegółów wyjątku (bezpieczeństwo!)
            // Szczegóły są w logach dla developerów
            var error = new Error(
                Code: "ServerError",
                Description: "Wystąpił nieoczekiwany błąd serwera. Spróbuj ponownie później.",
                Type: ErrorType.Critical);

            // Konwertuj na Result.Failure używając nowej fabryki
            return ResultFactory.CreateFailure<TResponse>(new[] { error });
        }
    }
}