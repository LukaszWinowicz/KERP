using KERP.Domain.Aggregates.User;
using KERP.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;

namespace KERP.BlazorUI.Extensions;

/// <summary>
/// Extension methods dla konfiguracji Authentication i Identity.
/// </summary>
public static class AuthenticationExtensions
{
    /// <summary>
    /// Konfiguruje ASP.NET Identity + Google Authentication dla Blazor Server.
    /// </summary>
    public static IServiceCollection AddAuthenticationWithGoogle(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ═══════════════════════════════════════════════════════════════════════════════
        // ASP.NET IDENTITY
        // ═══════════════════════════════════════════════════════════════════════════════
        services
            .AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Sign-in settings
                options.SignIn.RequireConfirmedAccount = false;
                options.SignIn.RequireConfirmedEmail = false;

                // Password settings (łagodne dla development)
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;

                // User settings
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        // ═══════════════════════════════════════════════════════════════════════════════
        // GOOGLE AUTHENTICATION
        // ═══════════════════════════════════════════════════════════════════════════════
        services
            .AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddGoogle(googleOptions =>
            {
                googleOptions.ClientId = configuration["Authentication:Google:ClientId"]!;
                googleOptions.ClientSecret = configuration["Authentication:Google:ClientSecret"]!;
                googleOptions.SaveTokens = true;

                // Scope
                googleOptions.Scope.Add("https://www.googleapis.com/auth/calendar.readonly");
                googleOptions.Scope.Add("https://www.googleapis.com/auth/user.addresses.read");
                googleOptions.Scope.Add("profile");
                googleOptions.Scope.Add("email");

                // Map claims
                googleOptions.ClaimActions.MapJsonKey("picture", "picture");
            });

        // ═══════════════════════════════════════════════════════════════════════════════
        // COOKIE CONFIGURATION
        // ═══════════════════════════════════════════════════════════════════════════════
        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Account/Login";
            options.LogoutPath = "/Account/Logout";
            options.AccessDeniedPath = "/Account/AccessDenied";
            options.ExpireTimeSpan = TimeSpan.FromDays(7);
            options.SlidingExpiration = true;
        });

        // ═══════════════════════════════════════════════════════════════════════════════
        // BLAZOR SERVER AUTHENTICATION STATE
        // ═══════════════════════════════════════════════════════════════════════════════
        services.AddCascadingAuthenticationState();
        services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

        return services;
    }
}