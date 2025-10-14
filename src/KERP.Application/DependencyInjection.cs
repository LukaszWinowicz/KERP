using KERP.Application.Common.Abstractions.CQRS;
using KERP.Application.Common.Behaviors;
using KERP.Application.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace KERP.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // ═══════════════════════════════════════════════════════════════════════════════
        // MEDIATOR - Centralny punkt wysyłania requestów
        // ═══════════════════════════════════════════════════════════════════════════════
        services.AddScoped<IMediator, Mediator>();

        // ═══════════════════════════════════════════════════════════════════════════════
        // COMMAND PIPELINE BEHAVIORS - w kolejności wykonania
        // ═══════════════════════════════════════════════════════════════════════════════

        // 1. LoggingBehavior - PIERWSZY (najbardziej zewnętrzny)
        //    Loguje start/koniec requestu + mierzy czas
        services.AddScoped(typeof(ICommandPipelineBehavior<,>), typeof(LoggingBehavior<,>));

        // 2. ValidationBehavior - Walidacja inputu
        //    Uruchamia wszystkie walidatory, przerywa jeśli błędy
        services.AddScoped(typeof(ICommandPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // 3. TransactionBehavior - Zarządzanie transakcją
        //    Begin/Commit/Rollback na podstawie Result
        services.AddScoped(typeof(ICommandPipelineBehavior<,>), typeof(TransactionBehavior<,>));

        // 4. ExceptionHandlingBehavior - OSTATNI (najbardziej wewnętrzny)
        //    Łapie wszystkie wyjątki i konwertuje na Result.Failure
        services.AddScoped(typeof(ICommandPipelineBehavior<,>), typeof(ExceptionHandlingBehavior<,>));

        // ═══════════════════════════════════════════════════════════════════════════════
        // QUERY PIPELINE BEHAVIORS - w kolejności wykonania
        // ═══════════════════════════════════════════════════════════════════════════════

        // 1. LoggingBehavior - PIERWSZY (najbardziej zewnętrzny)
        //    Loguje start/koniec requestu + mierzy czas
        //    UWAGA: Ten sam behavior co dla Command (współdzielony)
        services.AddScoped(typeof(IQueryPipelineBehavior<,>), typeof(LoggingBehavior<,>));

        // 2. ExceptionHandlingBehavior - OSTATNI (najbardziej wewnętrzny)
        //    Łapie wszystkie wyjątki i konwertuje na Result.Failure
        //    UWAGA: Ten sam behavior co dla Command (współdzielony)
        services.AddScoped(typeof(IQueryPipelineBehavior<,>), typeof(ExceptionHandlingBehavior<,>));

        // ═══════════════════════════════════════════════════════════════════════════════
        // HANDLERS & VALIDATORS - Automatyczna rejestracja przez Scrutor
        // ═══════════════════════════════════════════════════════════════════════════════

        // Używamy Scrutor do automatycznego znalezienia i zarejestrowania
        // wszystkich handlerów i walidatorów z projektu Application.
        services.Scan(selector => selector
            .FromAssemblies(typeof(DependencyInjection).Assembly)

            // Command Handlers
            .AddClasses(c => c.AssignableTo(typeof(ICommandHandler<,>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime()

            // Query Handlers
            .AddClasses(c => c.AssignableTo(typeof(IQueryHandler<,>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime()

            // Validators
            .AddClasses(c => c.AssignableTo(typeof(IValidator<>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

        return services;
    }
}