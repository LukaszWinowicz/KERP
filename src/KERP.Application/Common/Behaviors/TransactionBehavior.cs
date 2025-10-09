using KERP.Application.Common.Abstractions;
using KERP.Application.Common.Models;
using Microsoft.Extensions.Logging;

namespace KERP.Application.Common.Behaviors;

public class TransactionBehavior<TRequest, TResponse> : ICommandPipelineBehavior<TRequest, TResponse>
    where TResponse : Result
{
    private readonly IAppDbContext _dbContext;
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

    /// <summary>
    /// Inicjalizuje nową instancję TransactionBehavior.
    /// </summary>
    /// <param name="dbContext">Kontekst bazy danych używany do zarządzania transakcją.</param>
    /// <param name="logger">Logger do zapisywania informacji o transakcjach.</param>
    public TransactionBehavior(
        IAppDbContext dbContext,
        ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<TResponse> HandleAsync(
        TRequest request,
        Func<Task<TResponse>> next,
        CancellationToken cancellationToken = default)
    {
        // ═══════════════════════════════════════════════════════════════════════════════
        // KROK 1: Rozpocznij transakcję bazodanową
        // ═══════════════════════════════════════════════════════════════════════════════

        var requestName = typeof(TRequest).Name;

        // await using zapewnia że transakcja zostanie zdisposowana
        // (nawet jeśli wystąpi wyjątek)
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        _logger.LogInformation(
            "Started database transaction for request {RequestName}",
            requestName);

        try
        {
            // ═══════════════════════════════════════════════════════════════════════════
            // KROK 2: Wywołaj następny behavior/handler
            // ═══════════════════════════════════════════════════════════════════════════

            // Handler wykonuje się w kontekście transakcji
            // Wszystkie zmiany są śledzone przez EF Core, ale jeszcze nie zapisane
            var response = await next();

            // ═══════════════════════════════════════════════════════════════════════════
            // KROK 3: Sprawdź rezultat i zdecyduj: Commit czy Rollback
            // ═══════════════════════════════════════════════════════════════════════════

            if (response.IsSuccess)
            {
                // ───────────────────────────────────────────────────────────────────────
                // SUKCES - Commit transakcji
                // ───────────────────────────────────────────────────────────────────────

                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation(
                    "Committed database transaction for request {RequestName}",
                    requestName);
            }
            else
            {
                // ───────────────────────────────────────────────────────────────────────
                // PORAŻKA - Rollback transakcji
                // ───────────────────────────────────────────────────────────────────────

                // Result.IsFailure = true oznacza błąd biznesowy/walidacyjny
                // Nie chcemy zapisywać częściowych zmian
                await transaction.RollbackAsync(cancellationToken);

                var errorCodes = string.Join(", ", response.Errors.Select(e => e.Code));

                _logger.LogWarning(
                    "Rolled back database transaction for request {RequestName} due to failure: {ErrorCodes}",
                    requestName,
                    errorCodes);
            }

            return response;
        }
        catch (Exception ex)
        {
            // ═══════════════════════════════════════════════════════════════════════════
            // KROK 4: Obsługa wyjątku - Rollback i propaguj
            // ═══════════════════════════════════════════════════════════════════════════

            // Wyjątek oznacza nieoczekiwany błąd (np. SqlException, NullReferenceException)
            // Musimy wycofać transakcję aby nie zapisać częściowych zmian

            _logger.LogError(
                ex,
                "Rolling back database transaction for request {RequestName} due to exception",
                requestName);

            // Rollback transakcji
            // Użycie try-catch aby nie ukryć oryginalnego wyjątku
            try
            {
                await transaction.RollbackAsync(cancellationToken);
            }
            catch (Exception rollbackEx)
            {
                // Rollback sam rzucił wyjątek (bardzo rzadkie)
                // Logujemy, ale propagujemy oryginalny wyjątek (ex), nie rollbackEx
                _logger.LogError(
                    rollbackEx,
                    "Failed to rollback transaction for request {RequestName}",
                    requestName);
            }

            // Propaguj oryginalny wyjątek dalej
            // ExceptionHandlingBehavior go złapie i przekonwertuje na Result.Failure
            throw;
        }

        // ═══════════════════════════════════════════════════════════════════════════════
        // UWAGA: await using transaction zapewnia że transaction.Dispose() zostanie wywołane
        // nawet jeśli wystąpi wyjątek (cleanup zasobów)
        // ═══════════════════════════════════════════════════════════════════════════════
    }
}