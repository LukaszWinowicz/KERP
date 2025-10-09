namespace KERP.Application.Common.Abstractions;

/// <summary>
/// Definiuje kontrakt dla behavior'a w pipeline'ie przetwarzania COMMAND.
/// Pipeline behavior to wzorzec Chain ofResponsibility, który pozwala na
/// wykonanie logiki przed i po wywołaniu właściwego handlera komendy.
/// </summary>
/// <typeparam name="TCommand">Typ komendy implementującej ICommand lub ICommand<TResult>.</typeparam>
/// <typeparam name="TResponse">Typ odpowiedzi - zazwyczaj Result lub Result<TValue>.</typeparam>
public interface ICommandPipelineBehavior<in TCommand, TResponse>
{
    /// <summary>
    /// Wykonuje logikę behavior'a w pipeline'ie dla komendy.
    /// </summary>
    /// <param name="command">Komenda przechodząca przez pipeline.</param>
    /// <param name="next">Delegat do następnego ogniwa w łańcuchu.
    /// Wywołanie next() uruchamia kolejny bahavior lub właściwy handler.
    /// Można NIE wywoływać next() aby przerwać łańcuch (np. przy błędzie walidacji).</param>
    /// <param name="cancellationToken">Token anulowania operacji, propagowany przez cały pipeline.</param>
    /// <returns>Odpowiedź z pipeline'u. Behavior może:
    /// * Przekazać odpowiedź bez zmian
    /// * Zmodyfikować odpowiedź
    /// * Zwracać własną odpowiedź (np. Result.Failure przy błędzie).</returns>
    Task<TResponse> HandleAsync(
        TCommand command,
        Func<Task<TResponse>> next,
        CancellationToken cancellationToken = default);
}
