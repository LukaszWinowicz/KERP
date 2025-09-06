namespace KERP.Application.Common.Abstractions;

public interface ICommandDispatcher
{
    Task<TResult> Send<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default);
    Task Send(ICommand command, CancellationToken cancellationToken = default);
}
