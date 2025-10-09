namespace KERP.Application.Common.Abstractions;

/// <summary>
/// Definiuje kontrakt dla behavior'a w pipeline'ie przetwarzania QUERY.
/// Pipeline behavior to wzorzec Chain of Responsibility, który pozwala na
/// wykonanie logiki przed i po wywołaniu właściwego handlera zapytania.
/// </summary>
/// <typeparam name="TQuery">Typ zapytania implementującego IQuery&lt;TResult&gt;.</typeparam>
/// <typeparam name="TResponse">Typ odpowiedzi - zazwyczaj Result lub Result&lt;TValue&gt;.</typeparam>
/// <remarks>
/// Pipeline dla Query zazwyczaj zawiera:
/// 1. LoggingBehavior - logowanie rozpoczęcia/zakończenia
/// 2. CacheBehavior (opcjonalnie) - cache wyników zapytań
/// 3. ExceptionHandlingBehavior - konwersja wyjątków na Result.Failure
/// 
/// Query NIE powinno zawierać:
/// * ValidationBehavior - zapytania tylko czytają dane
/// * TransactionBehavior - zapytania nie modyfikują danych
/// </remarks>
public interface IQueryPipelineBehavior<in TQuery, TResponse>
{
    /// <summary>
    /// Wykonuje logikę behavior'a w pipeline'ie dla zapytania.
    /// </summary>
    /// <param name="query">Zapytanie przechodzące przez pipeline.</param>
    /// <param name="next">Delegat do następnego ogniwa w łańcuchu. 
    /// Wywołanie next() uruchamia kolejny behavior lub właściwy handler.
    /// Można NIE wywowływać next() aby przerwać łańcuch (np. przy cache hit).</param>
    /// <param name="cancellationToken">Token anulowania operacji, propagowany przez cały pipeline.</param>
    /// <returns>Odpowiedź z pipeline'u. Behavior może:
    /// * Przekazać odpowiedź bez zmian
    /// * Zwrócić zakeszowaną odpowiedź (bez wywołania handlera)
    /// * Zwrócić Result.Failure przy wyjątku.</returns>
    Task<TResponse> HandleAsync(
        TQuery query,
        Func<Task<TResponse>> next,
        CancellationToken cancellationToken = default);
}