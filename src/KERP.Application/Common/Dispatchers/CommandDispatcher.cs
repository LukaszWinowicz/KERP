using KERP.Application.Common.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace KERP.Application.Common.Dispatchers;

public sealed class CommandDispatcher : ICommandDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public CommandDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task Send(ICommand command, CancellationToken cancellationToken = default)
    {
        var handlerType = typeof(ICommandHandler<>).MakeGenericType(command.GetType());
        var handler = _serviceProvider.GetRequiredService(handlerType);

        return (Task)handlerType.GetMethod("Handle")!
            .Invoke(handler, new object[] { command, cancellationToken })!;
    }

    public Task<TResult> Send<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default)
    {
        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(command.GetType(), typeof(TResult));
        var handler = _serviceProvider.GetRequiredService(handlerType);

        return (Task<TResult>)handlerType.GetMethod("Handle")!
            .Invoke(handler, new object[] { command, cancellationToken })!;
    }
}