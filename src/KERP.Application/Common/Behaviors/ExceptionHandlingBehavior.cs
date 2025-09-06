using KERP.Application.Common.Abstractions;
using KERP.Application.Common.Models;
using KERP.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace KERP.Application.Common.Behaviors;

/// <summary>
/// Behavior (dekorator) przechwytujący wyjątki występujące podczas obsługi komend.
/// Tłumaczy wyjątki na odpowiednie obiekty Result.Failure.
/// </summary>
public class ExceptionHandlingBehavior<TCommand, TResult> : ICommandHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
    where TResult : Result
{
    private readonly ICommandHandler<TCommand, TResult> _decorated;
    private readonly ILogger<ExceptionHandlingBehavior<TCommand, TResult>> _logger;

    public ExceptionHandlingBehavior(
        ICommandHandler<TCommand, TResult> decorated,
        ILogger<ExceptionHandlingBehavior<TCommand, TResult>> logger)
    {
        _decorated = decorated;
        _logger = logger;
    }

    public async Task<TResult> Handle(TCommand command, CancellationToken cancellationToken)
    {
        try
        {
            // Przekazanie sterowania do następnego ogniwa w łańcuchu
            return await _decorated.Handle(command, cancellationToken);
        }
        // 1. Przechwycenie specyficznego, oczekiwanego wyjątku biznesowego
        catch (BusinessRuleValidationException ex)
        {
            // Logujemy jako ostrzeżenie, ponieważ jest to przewidywalny błąd logiki
            _logger.LogWarning("Business rule validation exception: {Message}", ex.Message);

            // Tworzymy błąd z komunikatem bezpiecznym dla użytkownika
            var error = new Error("BusinessRuleViolation", ex.Message, ErrorType.Critical);
            return Result.CreateFailure<TResult>(new[] { error });
        }
        // 2. Przechwycenie wszystkich innych, nieoczekiwanych wyjątków
        catch (Exception ex)
        {
            // Logujemy jako błąd krytyczny, ponieważ jest to awaria systemu
            _logger.LogError(ex, "An unhandled exception has occurred for command {CommandName}", typeof(TCommand).Name);

            // Tworzymy generyczny błąd, aby nie ujawniać szczegółów implementacji
            var error = new Error("ServerError", "Wystąpił nieoczekiwany błąd serwera.", ErrorType.Critical);
            return Result.CreateFailure<TResult>(new[] { error });
        }
    }

}