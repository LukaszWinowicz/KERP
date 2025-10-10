using KERP.Domain.Aggregates.User;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace KERP.BlazorUI;

/// <summary>
/// Custom AuthenticationStateProvider dla Blazor Server z ASP.NET Identity.
/// Rewaliduje użytkownika co określony czas, aby upewnić się że sesja jest aktualna.
/// </summary>
public class IdentityRevalidatingAuthenticationStateProvider
    : RevalidatingServerAuthenticationStateProvider
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IdentityOptions _options;

    public IdentityRevalidatingAuthenticationStateProvider(
        ILoggerFactory loggerFactory,
        IServiceScopeFactory scopeFactory,
        IOptions<IdentityOptions> optionsAccessor)
        : base(loggerFactory)
    {
        _scopeFactory = scopeFactory;
        _options = optionsAccessor.Value;
    }

    /// <summary>
    /// Określa jak często rewalidować użytkownika (domyślnie co 30 minut).
    /// </summary>
    protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(30);

    /// <summary>
    /// Sprawdza czy principal (zalogowany użytkownik) jest nadal ważny.
    /// </summary>
    protected override async Task<bool> ValidateAuthenticationStateAsync(
        AuthenticationState authenticationState,
        CancellationToken cancellationToken)
    {
        // Pobierz scope, aby uzyskać dostęp do UserManager
        await using var scope = _scopeFactory.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Sprawdź czy użytkownik nadal istnieje w bazie
        return await ValidateSecurityStampAsync(userManager, authenticationState.User);
    }

    /// <summary>
    /// Waliduje security stamp użytkownika.
    /// Security stamp zmienia się przy zmianie hasła lub danych wrażliwych.
    /// </summary>
    private async Task<bool> ValidateSecurityStampAsync(
        UserManager<ApplicationUser> userManager,
        ClaimsPrincipal principal)
    {
        var user = await userManager.GetUserAsync(principal);

        if (user == null)
        {
            return false;
        }

        // Jeśli nie ma security stamp claim, user jest ważny (external login)
        if (!principal.HasClaim(c => c.Type == _options.ClaimsIdentity.SecurityStampClaimType))
        {
            return true;
        }

        // Porównaj security stamp z claim ze stampem w bazie
        var principalStamp = principal.FindFirstValue(_options.ClaimsIdentity.SecurityStampClaimType);
        var userStamp = await userManager.GetSecurityStampAsync(user);

        return principalStamp == userStamp;
    }
}