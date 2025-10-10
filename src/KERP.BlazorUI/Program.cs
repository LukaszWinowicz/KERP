using KERP.Application;
using KERP.BlazorUI.Components;
using KERP.BlazorUI.Extensions;
using KERP.Infrastructure;
using Microsoft.FluentUI.AspNetCore.Components;
using Serilog;

// ═══════════════════════════════════════════════════════════════════════════════
// SERILOG CONFIGURATION
// ═══════════════════════════════════════════════════════════════════════════════
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ═══════════════════════════════════════════════════════════════════════════════
    // SERVICES CONFIGURATION
    // ═══════════════════════════════════════════════════════════════════════════════

    // Blazor
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();

    // FluentUI
    builder.Services.AddFluentUIComponents();

    // HttpContext (potrzebne dla CurrentUserService)
    builder.Services.AddHttpContextAccessor();

    // Application & Infrastructure layers
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    // Authentication with Google OAuth (z Extension Method!)
    builder.Services.AddAuthenticationWithGoogle(builder.Configuration);

    // MVC Controllers (potrzebne dla OAuth callbacks)
    builder.Services.AddMvcControllers();

    // ═══════════════════════════════════════════════════════════════════════════════
    // APP CONFIGURATION
    // ═══════════════════════════════════════════════════════════════════════════════
    var app = builder.Build();

    // Exception handling
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseAntiforgery();

    // Authentication & Authorization (WAŻNA KOLEJNOŚĆ!)
    app.UseAuthentication();
    app.UseAuthorization();

    // Map Razor Components
    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

    // Map Controllers (dla OAuth callbacks)
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Aplikacja nie mogła się uruchomić.");
}
finally
{
    Log.CloseAndFlush();
}