using KERP.Application.Common.Abstractions;
using KERP.Application.Common.Models;
using Microsoft.Extensions.Logging;
namespace KERP.Application.Common.Behaviors;

// Używamy generyków, aby ten behavior mógł obsłużyć dowolną komendę
public class LoggingBehavior<TCommand, TResult> : ICommandHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
    where TResult : Result
{
    private readonly ICommandHandler<TCommand, TResult> _decorated;
    private readonly ILogger<LoggingBehavior<TCommand, TResult>> _logger;

    public LoggingBehavior(
        ICommandHandler<TCommand, TResult> decorated,
        ILogger<LoggingBehavior<TCommand, TResult>> logger)
    {
        _decorated = decorated;
        _logger = logger;
    }

    public async Task<TResult> Handle(TCommand command, CancellationToken cancellationToken)
    {
        var commandName = command.GetType().Name;

        // Logowanie rozpoczęcia z użyciem parametrów do logania strukturalnego.
        _logger.LogInformation("Rozpoczęcie obsługi komendy {CommandName}", commandName);

        // Wywołanie następnego ogniwa w potoku.
        var result = await _decorated.Handle(command, cancellationToken);

        // Analiza wyników i odpowiednie logowanie.
        if (result.IsSuccess)
        {
            _logger.LogInformation("Pomyślnie zakończono obsługę komendy {CommandName}", commandName);
            return result;
        }
        else
        {
            // Jeśli operacja zawidoła (np. błąd walidacji), logujemy to jako ostrzeżenie.
            // Łączymy błędy w jeden string dla czytelności logu.
            var errorsText = string.Join(" | ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
            _logger.LogWarning("Obsługa komendy {CommandName} zakończona błędem: {Errors}", commandName, errorsText);
        }

        // Zwracamy wynik, nawet jeśli jest błędny, aby umożliwić dalsze przetwarzanie.
        return result;
    }
}
