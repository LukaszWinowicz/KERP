using KERP.BlazorUI.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using KERP.Application;
using KERP.Infrastructure;
using Serilog;

// Konfiguracja Seriloga
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning) // Ustawienie minimalnego poziomu logowania dla Microsoft (ukrywa szym z logów systemowych)
    .Enrich.FromLogContext() // Kluczowe dla Correlation ID
    .WriteTo.Console() // Kieruje logi do konsoli
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();
    builder.Services.AddFluentUIComponents();

    builder.Services.AddHttpContextAccessor();

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseHttpsRedirection();

    app.UseStaticFiles();
    app.UseAntiforgery();

    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

    app.Run();

}
catch (Exception ex)
{
    Log.Fatal(ex, "Aplikacja nie mog³a siê uruchomiæ");
}
finally
{
    Log.CloseAndFlush(); // Upewnij siê, ¿e wszystkie logi s¹ zapisane przed zakoñczeniem aplikacji
}