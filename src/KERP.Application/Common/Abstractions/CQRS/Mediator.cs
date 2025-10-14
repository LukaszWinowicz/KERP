using Microsoft.Extensions.DependencyInjection;

namespace KERP.Application.Common.Abstractions.CQRS;

/// <summary>
/// Implementacja mediatora odpowiedzialnego za routing requestów do odpowiednich handlerów
/// oraz budowanie i wykonywanie pipelin'u z behaviors.
/// </summary>
public sealed class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Inicjalizuje nową instację mediatora.
    /// </summary>
    /// <param name="serviceProvider">Service Provider używany do rozwiązywania hanlderów i behaviors z DI container.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <inheritdoc />
    public Task<TResult> SendCommandAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default) where TCommand : ICommand<TResult>
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        var handler = _serviceProvider.GetService<ICommandHandler<TCommand, TResult>>()
            ?? throw new InvalidOperationException($"No handler registered for command '{typeof(TCommand).Name}'.");

        var behaviors = _serviceProvider.GetServices<ICommandPipelineBehavior<TCommand, TResult>>();

        var pipeline = BuildPipeline(
            command,
            () => handler.HandleAsync(command, cancellationToken),
            behaviors,
            cancellationToken);

        return pipeline();
    }

    /// <inheritdoc />
    public Task<TResult> SendQueryAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default) where TQuery : IQuery<TResult>
    {
        if (query == null)
            throw new ArgumentNullException(nameof(query));

        var handler = _serviceProvider.GetService<IQueryHandler<TQuery, TResult>>()
            ?? throw new InvalidOperationException($"No handler registered for query '{typeof(TQuery).Name}'.");

        var behaviors = _serviceProvider.GetServices<IQueryPipelineBehavior<TQuery, TResult>>();

        var pipeline = BuildPipeline(
            query,
            () => handler.HandleAsync(query, cancellationToken),
            behaviors,
            cancellationToken);

        return pipeline();
    }

    /// <summary>
    /// Buduje pipeline wywołań (łańcuch odpowiedzialności) dla danego requestu.
    /// </summary>
    private static Func<Task<TResult>> BuildPipeline<TRequest, TResult>(
        TRequest request,
        Func<Task<TResult>> handlerFunc,
        IEnumerable<IPipelineBehavior<TRequest, TResult>> behaviors,
        CancellationToken cancellationToken)
    {
        // Odwracamy kolekcję, aby złożyć pipeline od wewnątrz do zewnątrz
        // Ostatni behavior będzie wywołany jako pierwszy.
        foreach (var behavior in behaviors.Reverse())
        {
            var currentBehavior = behavior;
            var next = handlerFunc;

            // Tworzymy nowy delegat, który zamyka w sobie aktualny behavior
            // i delegat do następnego kroku w pipeline'ie ('next').
            handlerFunc = () => currentBehavior.HandleAsync(request, next, cancellationToken);
        }

        return handlerFunc;
    }
}