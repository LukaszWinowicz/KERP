using KERP.Application.Common.Abstractions;
using KERP.Application.Services;
using KERP.Infrastructure.Persistence;
using KERP.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KERP.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Rejestracja DbContext
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Mówimy kontenerowi, że interfejs IAppDbContext jest realizowany przez klasę AppDbContext.
        // On sam zrozumie, żeby użyć tej samej instancji, co dla wstrzyknięcia samego AppDbContext.
        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        // Rejestracja naszych implementacji jako Scoped
        // Oznacza to, że jeden obiekt będzie używany w ramach jednego żądania HTTP.        
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }
}