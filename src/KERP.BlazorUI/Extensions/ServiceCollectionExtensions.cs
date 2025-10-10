namespace KERP.BlazorUI.Extensions;

/// <summary>
/// Extension methods dla konfiguracji services w BlazorUI.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Dodaje MVC Controllers (potrzebne dla OAuth callbacks).
    /// </summary>
    public static IServiceCollection AddMvcControllers(this IServiceCollection services)
    {
        services.AddControllers();
        return services;
    }
}
