namespace KERP.Application.Common.Abstractions.CQRS;

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
    : IPipelineBehavior<TQuery, TResponse>
{
}