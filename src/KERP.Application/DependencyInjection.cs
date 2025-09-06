using KERP.Application.Common.Abstractions;
using KERP.Application.Common.Dispatchers;
using KERP.Application.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace KERP.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Rejestracja Dispatcherów
        services.AddScoped<ICommandDispatcher, CommandDispatcher>();
        services.AddScoped<IQueryDispatcher, QueryDispatcher>();

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