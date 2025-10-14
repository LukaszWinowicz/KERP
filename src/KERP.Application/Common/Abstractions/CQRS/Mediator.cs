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
    public async Task<TResult> SendCommandAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default) where TCommand : ICommand<TResult>
    {
        // KROK 1: Walidacja wejścia
        if (command == null)
        {
            throw new ArgumentNullException(nameof(command),
                "Command cannot be null. Ensure you pass a valid command instance.");
        }

        // KROK 2: Resolve handler z DI
        // GetService<T>() - zwraca null jeśli nie znaleziono
        // GetRequiredService<T>() - rzuca wyjątek jeśli nie znaleziono (preferowane)
        var handler = _serviceProvider.GetService<ICommandHandler<TCommand, TResult>>();

        if (handler == null)
        {
            throw new InvalidOperationException(
                $"No handler registered for command '{typeof(TCommand).Name}'. " +
                $"Ensure that ICommandHandler<{typeof(TCommand).Name}, {typeof(TResult).Name}> " +
                "is registered in the DI container.");
        }

        // KROK 3: Resolve wszystkich behaviors dla tego typu requestu
        // GetServices<T>() zwraca IEnumerable<T> - wszystkie zarejestrowane implementacje
        // Kolejność jest określona przez kolejność rejestracji w DI
        var behaviors = _serviceProvider
            .GetServices<ICommandPipelineBehavior<TCommand, TResult>>()
            .ToList(); // ToList() aby zmaterializować kolekcję

        // KROK 4: Build pipeline - budowanie łańucha wywołań.

        // To jest "rdzeń cebuli" - właściwy handler który wykonuje logikę biznesową
        Func<Task<TResult>> handlerFunc = () => handler.HandleAsync(command, cancellationToken);

        foreach (var behavior in behaviors.AsEnumerable().Reverse())
        {
            // Capture zmiennych w closure - ważne dla async/await
            var currentBehavior = behavior;
            var next = handlerFunc;

            // Tworzymy nowy delegate, który wywołuje behavior z poprzednim delegate jako 'next'
            handlerFunc = () => currentBehavior.HandleAsync(command, next, cancellationToken);
        }

        // KROK 5: Wykonanie pipeline

        // handlerFunc() uruchamia pierwszy behavior (np. Logging)
        // Ten behavior wywołuje next(), który uruchamia kolejny behavior (np. Validation)
        // I tak dalej, aż do handlera
        // Potem wszystko "wraca" w odwrotnej kolejności (jak cebula)
        return await handlerFunc();
    }

    /// <inheritdoc />
    public async Task<TResult> SendQueryAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default) where TQuery : IQuery<TResult>
    {
        // KROK 1: Walidacja wejścia
        if (query == null)
        {
            throw new ArgumentNullException(nameof(query),
                "Query cannot be null. Ensure you pass a valid query instance.");
        }

        // KROK 2: Resolve handlera z DI
        var handler = _serviceProvider.GetService<IQueryHandler<TQuery, TResult>>();

        if (handler == null)
        {
            throw new InvalidOperationException(
                $"No handler registered for query '{typeof(TQuery).Name}'. " +
                $"Ensure that IQueryHandler<{typeof(TQuery).Name}, {typeof(TResult).Name}> " +
                "is registered in the DI container.");
        }

        // KROK 3: Resolve wszystkich behaviors dla tego typu requestu
        // Query pipeline zazwyczaj ma inne behaviors niż Command:
        // - Command: Logging, Validation, Transaction, Exception
        // - Query: Logging, Cache, Exception
        var behaviors = _serviceProvider
            .GetServices<IQueryPipelineBehavior<TQuery, TResult>>()
            .ToList();

        // KROK 4: Build pipeline
        // Dokładnie ta sama logika jak w SendCommandAsync
        Func<Task<TResult>> handlerFunc = () => handler.HandleAsync(query, cancellationToken);

        foreach (var behavior in behaviors.AsEnumerable().Reverse())
        {
            var currentBehavior = behavior;
            var next = handlerFunc;
            handlerFunc = () => currentBehavior.HandleAsync(query, next, cancellationToken);
        }

        // KROK 5: Wykonanie pipeline
        return await handlerFunc();

    }
}
