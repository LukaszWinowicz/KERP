using KERP.Application.Common.Abstractions;
using KERP.Application.Common.Models;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
namespace KERP.Application.Common.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TResponse : class
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    /// <summary>
    /// Inicjalizuje nową instancję LoggingBehavior.
    /// </summary>
    /// <param name="logger">Logger do zapisywania informacji o requestach.</param>
    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<TResponse> HandleAsync(
        TRequest request,
        Func<Task<TResponse>> next,
        CancellationToken cancellationToken = default)
    {
        // PRZED wykonaniem requestu - Log rozpoczęcia.
        var requestName = typeof(TRequest).Name;

        _logger.LogInformation(
            "Starting request: {RequestName}",
            requestName);

        // Rozpocznij mierzenie czasu
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Wywołaj następny behavior lub handler
            var response = await next();

            // Po wykonaniu requestu - Log zakończenia
            stopwatch.Stop();

            // Sprawdź czy response to Result (aby wyciągnąć informacje o sukcesie/błędach)
            if (response is Result result)
            {
                if (result.IsSuccess)
                {
                    // Sukces - loguj jako Information
                    _logger.LogInformation(
                        "Completed request: {RequestName} in {ElapsedMilliseconds}ms (Success)",
                        requestName,
                        stopwatch.ElapsedMilliseconds);
                }
                else
                {
                    // Błąd (np. walidacja) - loguj jako Warning z kodami błędów
                    var errorCodes = string.Join(", ", result.Errors.Select(e => e.Code));

                    _logger.LogWarning(
                        "Completed request: {RequestName} in {ElapsedMilliseconds}ms (Failure: {ErrorCodes})",
                        requestName,
                        stopwatch.ElapsedMilliseconds,
                        errorCodes);
                }
            }
            else
            {
                // Response nie jest Result - loguj po prostu sukces
                _logger.LogInformation(
                    "Completed request: {RequestName} in {ElapsedMilliseconds}ms",
                    requestName,
                    stopwatch.ElapsedMilliseconds);
            }

            return response;

        }
        catch (Exception ex)
        {

            // Wyjątek (nieoczekiwany błąd) - Log jako Error

            stopwatch.Stop();

            _logger.LogError(
                ex,
                "Request {RequestName} failed after {ElapsedMilliseconds}ms with exception",
                requestName,
                stopwatch.ElapsedMilliseconds);

            // Propaguj wyjątek dalej - zostanie złapany przez ExceptionHandlingBehavior
            throw;
        }

    }
}
