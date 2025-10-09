using KERP.Application.Common.Abstractions;
using KERP.Application.Common.Behaviors;
using KERP.Application.Common.Dispatchers;
using KERP.Application.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace KERP.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // MEDIATOR - Centralny punkt wysyłania requestów.
        services.AddScoped<IMediator, Mediator>();

        // PIPELINE BEHAVIORS - w kolejności wykonania.
        // 1. LoggingBehavior - PIERWSZY (najbardziej zewnętrzny)
        //    Loguje start/koniec requestu + mierzy czas
        //    Działa dla: Command + Query
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

        // 2. ValidationBehavior - Walidacja inputu
        //    Uruchamia wszystkie walidatory, przerywa jeśli błędy
        //    Działa dla: Command ONLY
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // 3. TransactionBehavior - Zarządzanie transakcją
        //    Begin/Commit/Rollback na podstawie Result
        //    Działa dla: Command ONLY
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

        // 4. ExceptionHandlingBehavior - OSTATNI (najbardziej wewnętrzny)
        //    Łapie wszystkie wyjątki i konwertuje na Result.Failure
        //    Działa dla: Command + Query
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingBehavior<,>));

        // HANDLERS & VALIDATORS - Automatyczna rejestracja przez Scrutor

        // Używamy Scrutor do automatycznego znalezienia i zarejestrowania
        // wszystkich handlerów i walidatorów z projektu Application.
        services.Scan(selector => selector
            .FromAssemblies(typeof(DependencyInjection).Assembly)
            .AddClasses(c => c.AssignableTo(typeof(ICommandHandler<,>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            .AddClasses(c => c.AssignableTo(typeof(IQueryHandler<,>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            .AddClasses(c => c.AssignableTo(typeof(IValidator<>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

        return services;
    }
}