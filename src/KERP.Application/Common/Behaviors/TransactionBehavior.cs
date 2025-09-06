using KERP.Application.Common.Abstractions;
using KERP.Application.Common.Models;
using Microsoft.Extensions.Logging;

namespace KERP.Application.Common.Behaviors;

/// <summary>
/// Behavior (dekorator) odpowiedzalny za zarządzanie transakcjami bazowymi.
/// Używa standardowego mechanizmu transakcji z EF Core.
/// </summary>
public class TransactionBehavior<TCommadn, TResult> : ICommandHandler<TCommadn, TResult>
    where TCommadn : ICommand<TResult>
    where TResult : Result
{
    private readonly ICommandHandler<TCommadn, TResult> _decorated;
    private readonly IAppDbContext _dbContext;
    private readonly ILogger<TransactionBehavior<TCommadn, TResult>> _logger;
    public TransactionBehavior(
        ICommandHandler<TCommadn, TResult> decorated,
        IAppDbContext dbContext,
        ILogger<TransactionBehavior<TCommadn, TResult>> logger)
    {
        _decorated = decorated;
        _dbContext = dbContext;
        _logger = logger;
    }
    public async Task<TResult> Handle(TCommadn command, CancellationToken cancellationToken)
    {
        // Rozpoczęcie transakcji na poziomie bazy danych
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        _logger.LogInformation("Rozpoczęto transakcję dla komendy {CommandName}", typeof(TCommadn).Name);

        try
        {
            // Wywołuje następne ogniwo (czyli właściwy CommandHandler)
            var result = await _decorated.Handle(command, cancellationToken);

            // Transakcja jest zatwierdzona tylko wtedy, gdy wewnętrzny handler zwrócił sukces.
            if (result.IsSuccess)
            {
                // Zatwierdzenie transakcji, jeśli operacja w handlerze zakończyła się sukcesem.
                await transaction.CommitAsync(cancellationToken);
                _logger.LogInformation("Transakcja dla komendy {CommandName} została zatwierdzona (commit).", typeof(TCommadn).Name);
            }
            else
            {
                // Jeśli handler zwróci błąd (Result.Failure), ale nie wyrzuci wyjątku,
                // to transakcja jest wycofywana aby nie zapisać częściowo danych.           
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogWarning("Transakcja dla komendy {CommandName} została wycofana (rollback) z powodu błędu: {Errors}",
                                    typeof(TCommadn).Name, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
            return result;
        }
        catch (Exception ex)
        {
            // Jeśli w trakcie operacji wystąpił jakikolwiek wyjątek, wycofujemy transakcję i logujemy błąd.
            _logger.LogError(ex, "Wystąpił błąd podczas przetwarzania komendy {CommandName}", typeof(TCommadn).Name);
            await transaction.RollbackAsync(cancellationToken);

            // Rzucamy wyjątek dalej, aby został złapany przez ExceptionHandlingBehavior.
            throw;
        }
    }
}