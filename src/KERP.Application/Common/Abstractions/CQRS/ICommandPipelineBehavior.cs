namespace KERP.Application.Common.Abstractions.CQRS;

/// <summary>
/// Definiuje kontrakt dla behavior'a w pipeline'ie przetwarzania COMMAND.
/// Pipeline behavior to wzorzec Chain ofResponsibility, który pozwala na
/// wykonanie logiki przed i po wywołaniu właściwego handlera komendy.
/// </summary>
/// <typeparam name="TCommand">Typ komendy implementującej ICommand lub ICommand<TResult>.</typeparam>
/// <typeparam name="TResponse">Typ odpowiedzi - zazwyczaj Result lub Result&lt;TValue&gt;.</typeparam>
public interface ICommandPipelineBehavior<in TCommand, TResponse>
    : IPipelineBehavior<TCommand, TResponse>
{
}
