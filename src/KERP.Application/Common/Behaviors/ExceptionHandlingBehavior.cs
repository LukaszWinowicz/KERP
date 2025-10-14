using KERP.Application.Common.Abstractions.CQRS;
using KERP.Application.Common.Models;
using KERP.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using System.Reflection;

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

            // Konwertuj na Result.Failure
            return CreateFailureResult(new[] { error });
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

            // Konwertuj na Result.Failure
            return CreateFailureResult(new[] { error });
        }
    }

    /// <summary>
    /// Tworzy Result.Failure dla typu TResponse (Result lub Result&lt;T&gt;).
    /// </summary>
    /// <param name="errors">Lista błędów.</param>
    /// <returns>Instancja TResponse reprezentująca porażkę.</returns>
    /// <remarks>
    /// Ta metoda używa reflection aby stworzyć odpowiedni typ Result:
    /// - Dla TResponse = Result → Result.Failure(errors)
    /// - Dla TResponse = Result&lt;ProductDto&gt; → Result&lt;ProductDto&gt;.Failure(errors)
    /// 
    /// Reflection jest potrzebne bo system typów C# nie pozwala na bezpośrednie użycie
    /// generycznego typu TResponse w tym kontekście (limitation kompilatora).
    /// </remarks>
    private static TResponse CreateFailureResult(IReadOnlyCollection<Error> errors)
    {
        var responseType = typeof(TResponse);

        // ═══════════════════════════════════════════════════════════════════════════════
        // PRZYPADEK 1: TResponse = Result (bez generyka)
        // ═══════════════════════════════════════════════════════════════════════════════
        if (responseType == typeof(Result))
        {
            // Prosty cast - Result.Failure zwraca Result
            var result = Result.Failure(errors);
            return (TResponse)(object)result;
        }

        // ═══════════════════════════════════════════════════════════════════════════════
        // PRZYPADEK 2: TResponse = Result<T> (generyczny)
        // ═══════════════════════════════════════════════════════════════════════════════
        if (responseType.IsGenericType &&
            responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            // Wyciągnij typ T z Result<T>
            // Np. dla Result<ProductDto> → valueType = ProductDto
            var valueType = responseType.GetGenericArguments()[0];

            // Zbuduj typ Result<T> w runtime
            // typeof(Result<>) + ProductDto → Result<ProductDto>
            var resultType = typeof(Result<>).MakeGenericType(valueType);

            // Znajdź metodę statyczną Failure na Result<ProductDto>
            var failureMethod = resultType.GetMethod(
                "Failure",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(IReadOnlyCollection<Error>) },
                null);

            if (failureMethod == null)
            {
                throw new InvalidOperationException(
                    $"Could not find Failure method on type {resultType.Name}. " +
                    "Ensure Result<T> has a public static Failure method.");
            }

            // Wywołaj Result<ProductDto>.Failure(errors)
            var result = failureMethod.Invoke(null, new object[] { errors });

            if (result == null)
            {
                throw new InvalidOperationException(
                    $"Failure method on {resultType.Name} returned null.");
            }

            // Cast do TResponse
            return (TResponse)result;
        }

        // ═══════════════════════════════════════════════════════════════════════════════
        // PRZYPADEK 3: TResponse to coś innego (błąd)
        // ═══════════════════════════════════════════════════════════════════════════════
        throw new InvalidOperationException(
            $"Unsupported result type: {responseType.Name}. " +
            "ExceptionHandlingBehavior requires TResponse to be Result or Result<T>.");
    }

}