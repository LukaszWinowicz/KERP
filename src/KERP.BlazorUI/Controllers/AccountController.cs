using KERP.Domain.Aggregates.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KERP.BlazorUI.Controllers;

/// <summary>
/// Controller obsługujący logowanie zewnętrzne (Google OAuth).
/// </summary>
[Route("[controller]/[action]")]
public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ILogger<AccountController> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Inicjuje external login (redirect do Google).
    /// </summary>
    [HttpPost]
    public IActionResult ExternalLogin(string provider, string? returnUrl = null)
    {
        // Zbuduj URL do callback
        var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });

        // Skonfiguruj properties dla external authentication
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

        // Redirect do Google OAuth
        return Challenge(properties, provider);
    }

    /// <summary>
    /// Callback po powrocie z Google OAuth.
    /// URL: /Account/ExternalLoginCallback
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
    {
        returnUrl ??= "/";

        if (remoteError != null)
        {
            _logger.LogWarning("External login error: {Error}", remoteError);
            return LocalRedirect($"/Account/Login?ErrorMessage={Uri.EscapeDataString($"Error from external provider: {remoteError}")}");
        }

        // ═══════════════════════════════════════════════════════════════════════════════
        // KROK 1: Pobierz informacje o użytkowniku z Google
        // ═══════════════════════════════════════════════════════════════════════════════
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            _logger.LogWarning("Could not load external login info");
            return LocalRedirect("/Account/Login?ErrorMessage=Error loading external login information");
        }

        // ═══════════════════════════════════════════════════════════════════════════════
        // KROK 2: Sprawdź czy użytkownik już istnieje (po provider key)
        // ═══════════════════════════════════════════════════════════════════════════════
        var result = await _signInManager.ExternalLoginSignInAsync(
            info.LoginProvider,
            info.ProviderKey,
            isPersistent: false,
            bypassTwoFactor: true);

        if (result.Succeeded)
        {
            _logger.LogInformation("User logged in with {Provider} provider", info.LoginProvider);
            return LocalRedirect(returnUrl);
        }

        if (result.IsLockedOut)
        {
            _logger.LogWarning("User account locked out");
            return LocalRedirect("/Account/Login?ErrorMessage=Account locked");
        }

        // ═══════════════════════════════════════════════════════════════════════════════
        // KROK 3: Użytkownik loguje się pierwszy raz - utwórz konto
        // ═══════════════════════════════════════════════════════════════════════════════

        // Pobierz email z Google
        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrEmpty(email))
        {
            _logger.LogWarning("Email not found in external login info");
            return LocalRedirect("/Account/Login?ErrorMessage=Email not available from Google");
        }

        // Sprawdź czy użytkownik z tym emailem już istnieje
        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            // Użytkownik istnieje, ale nie ma powiązanego external login
            // Dodaj external login do istniejącego użytkownika
            var addLoginResult = await _userManager.AddLoginAsync(existingUser, info);
            if (addLoginResult.Succeeded)
            {
                await _signInManager.SignInAsync(existingUser, isPersistent: false);
                _logger.LogInformation("External login added to existing user {Email}", email);
                return LocalRedirect(returnUrl);
            }
            else
            {
                _logger.LogError("Failed to add external login to user {Email}", email);
                return LocalRedirect("/Account/Login?ErrorMessage=Error linking external login");
            }
        }

        // Utwórz nowego użytkownika
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true, // Google już zweryfikował email
            FactoryId = null // Domyślnie null, user wybierze później
        };

        var createResult = await _userManager.CreateAsync(user);
        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            _logger.LogError("Failed to create user {Email}: {Errors}", email, errors);
            return LocalRedirect($"/Account/Login?ErrorMessage={Uri.EscapeDataString($"Error creating account: {errors}")}");
        }

        // Dodaj external login do nowego użytkownika
        var addLoginToNewUserResult = await _userManager.AddLoginAsync(user, info);
        if (!addLoginToNewUserResult.Succeeded)
        {
            _logger.LogError("Failed to add external login to new user {Email}", email);
            return LocalRedirect("/Account/Login?ErrorMessage=Error linking external login");
        }

        // Zaloguj użytkownika
        await _signInManager.SignInAsync(user, isPersistent: false);
        _logger.LogInformation("New user created and logged in with {Provider}: {Email}", info.LoginProvider, email);

        return LocalRedirect(returnUrl);
    }

    /// <summary>
    /// Wylogowanie użytkownika.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Logout(string? returnUrl = null)
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User logged out");

        if (!string.IsNullOrEmpty(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return LocalRedirect("/");
    }
}