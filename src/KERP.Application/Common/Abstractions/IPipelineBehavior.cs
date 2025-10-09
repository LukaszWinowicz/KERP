namespace KERP.Application.Common.Abstractions;

/// <summary>
/// Definiuje kontrakt dla behavior'a w pipeline'ie przetwarzania requestów.
/// Pipeline behavior to wzorzec Chain of Responsibility, ktory pozwala na
/// wykonanie logiki przed i po wywołaniu właściwego handlera.
/// </summary>
/// <typeparam name="TRequest">Typ żądania (request) - może być ICommand lub IQuery.</typeparam>
/// <typeparam name="TResponse">Typ odpowiedzi (response) - zazwyczaj Result lub Result<TValue>.</typeparam>
public interface IPipelineBehavior<in TRequest, TResponse>
{
    /// <summary>
    /// Wykonuje logikę behavior'a w pipeline'ie.
    /// </summary>
    /// <param name="request">Żądanie (request) przechodzące przez pipeline. Może być typu ICommand<TResult> lub IQuery<TResult>.</param>
    /// <param name="next">Delegat do następnego ogniwa w łańcuchu. Wywołanie next() uruchamia kolejny behavior lub właściwy handler.
    /// Można NIE wywowływać next() aby przerwać łańcuch (np. przy błędzie walidacji lub cache hit).</param>
    /// <param name="cancellationToken">Token anulowani operacji, propagowany przez cały pipeline.</param>
    /// <returns>Odpowiedź (response) z pipelin'u. Behavior może:
    /// * Przekazać odpowiedź bez zmian
    /// * Zmodyfikować odpowiedź
    /// * Zwrócić własną odpowiedz (np. Result.Failure przy błędzie).</returns>
    Task<TResponse> HandleAsync(
        TRequest request,
        Func<Task<TResponse>> next,
        CancellationToken cancellationToken = default);
}
