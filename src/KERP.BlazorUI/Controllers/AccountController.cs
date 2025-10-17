using KERP.Application.Common.Abstractions.Repositories;
using KERP.Application.Services;
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
    private readonly IFactoryRepository _factoryRepository;
    private readonly IUserClaimsService _userClaimsService;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ILogger<AccountController> logger,
        IFactoryRepository factoryRepository,
        IUserClaimsService userClaimsService)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
        _factoryRepository = factoryRepository;
        _userClaimsService = userClaimsService;
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
        // KROK 2: Wyciągnij wszystkie przydatne informacje z Google Claims
        // ═══════════════════════════════════════════════════════════════════════════════

        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        var fullName = info.Principal.FindFirstValue(ClaimTypes.Name) ?? email?.Split('@')[0] ?? "User";
        var givenName = info.Principal.FindFirstValue(ClaimTypes.GivenName);
        var surname = info.Principal.FindFirstValue(ClaimTypes.Surname);

        // Loguj otrzymane claims dla debugowania
        _logger.LogInformation("Google claims received - Email: {Email}, Name: {Name}, GivenName: {GivenName}, Surname: {Surname}",
            email, fullName, givenName, surname);

        if (string.IsNullOrEmpty(email))
        {
            _logger.LogWarning("Email not found in external login info");
            return LocalRedirect("/Account/Login?ErrorMessage=Email not available from Google");
        }


        // ═══════════════════════════════════════════════════════════════════════════════
        // KROK 3: Sprawdź czy użytkownik już istnieje (po provider key)
        // ═══════════════════════════════════════════════════════════════════════════════

        // Najpierw próba zalogowania istniejącego użytkownika
        var signInResult = await _signInManager.ExternalLoginSignInAsync(
            info.LoginProvider,
            info.ProviderKey,
            isPersistent: false,
            bypassTwoFactor: true);

        ApplicationUser user;

        if (signInResult.Succeeded)
        {
            // ═══════════════════════════════════════════════════════════════════════════
            // KROK 3A: Użytkownik istnieje - zaktualizuj jego claims
            // ═══════════════════════════════════════════════════════════════════════════

            _logger.LogInformation("Existing user logging in with {Provider}: {Email}", info.LoginProvider, email);

            // Pobierz użytkownika z bazy
            user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                _logger.LogError("User signed in but could not be found in database");
                return LocalRedirect("/Account/Login?ErrorMessage=User account error");
            }

            // ═══════════════════════════════════════════════════════════════════════════
            // Auto-repair FactoryId jeśli jest NULL
            // ═══════════════════════════════════════════════════════════════════════════

            if (!user.FactoryId.HasValue)
            {
                _logger.LogWarning(
                    "User {Email} has NULL FactoryId. Attempting to restore default factory.",
                    email);

                const int DEFAULT_FACTORY_ID = 241;

                // Sprawdź czy domyślna fabryka istnieje i jest aktywna
                var defaultFactoryExists = await _factoryRepository.ExistsAndIsActiveAsync(DEFAULT_FACTORY_ID);

                if (defaultFactoryExists)
                {
                    // Przywróć domyślną fabrykę
                    user.FactoryId = DEFAULT_FACTORY_ID;
                    var updateResult = await _userManager.UpdateAsync(user);

                    if (updateResult.Succeeded)
                    {
                        _logger.LogInformation(
                            "Successfully restored FactoryId={FactoryId} for user {Email}",
                            DEFAULT_FACTORY_ID,
                            email);
                    }
                    else
                    {
                        var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                        _logger.LogError(
                            "Failed to restore FactoryId for user {Email}: {Errors}",
                            email,
                            errors);

                        // Kontynuuj logowanie mimo błędu - user zobaczy walidację później
                    }
                }
                else
                {
                    _logger.LogError(
                        "Cannot restore FactoryId for user {Email}: Default factory {FactoryId} does not exist or is inactive",
                        email,
                        DEFAULT_FACTORY_ID);

                    // Kontynuuj logowanie - user zobaczy błąd walidacji przy próbie operacji
                }
            }
            else
            {
                // User ma FactoryId - sprawdź czy jest aktywna
                var isFactoryActive = await _factoryRepository.ExistsAndIsActiveAsync(user.FactoryId.Value);

                if (!isFactoryActive)
                {
                    _logger.LogWarning(
                        "User {Email} has FactoryId={FactoryId} but factory is inactive or doesn't exist",
                        email,
                        user.FactoryId.Value);

                    // Opcja A: Wymuś zmianę fabryki na domyślną
                    // Opcja B: Wyloguj użytkownika z komunikatem
                    // Na razie: kontynuuj, walidacja przechwyci później
                }
            }

            // Zaktualizuj claims dla istniejącego użytkownika
            await _userClaimsService.UpdateUserClaimsAsync(user, fullName, email);

            return LocalRedirect(returnUrl);
        }

        if (signInResult.IsLockedOut)
        {
            _logger.LogWarning("User account locked out");
            return LocalRedirect("/Account/Login?ErrorMessage=Account locked");
        }

        // ═══════════════════════════════════════════════════════════════════════════════
        // KROK 3B: Nowy użytkownik - utwórz konto
        // ═══════════════════════════════════════════════════════════════════════════════

        // Sprawdź czy użytkownik z tym emailem już istnieje (ale bez external login)
        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            // Użytkownik istnieje, dodaj external login
            var addLoginResult = await _userManager.AddLoginAsync(existingUser, info);
            if (addLoginResult.Succeeded)
            {
                await _userClaimsService.UpdateUserClaimsAsync(existingUser, fullName, email);
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
        user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true, // Google już zweryfikował email
            FactoryId = 241 // Domyślna fabryka - w przyszłości można to zmienić na null i prosić o wybór
        };

        if (user.FactoryId.HasValue)
        {
            var factoryExists = await _factoryRepository.ExistsAndIsActiveAsync(user.FactoryId.Value);
            if (!factoryExists)
            {
                _logger.LogWarning(
                    "Default factory {FactoryId} does not exist or is inactive, setting to null",
                    user.FactoryId.Value);
                user.FactoryId = null;
            }
        }

        var createResult = await _userManager.CreateAsync(user);
        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            _logger.LogError("Failed to create user {Email}: {Errors}", email, errors);
            return LocalRedirect($"/Account/Login?ErrorMessage={Uri.EscapeDataString($"Error creating account: {errors}")}");
        }

        // Dodaj external login
        var addLoginToNewUserResult = await _userManager.AddLoginAsync(user, info);
        if (!addLoginToNewUserResult.Succeeded)
        {
            _logger.LogError("Failed to add external login to new user {Email}", email);
            return LocalRedirect("/Account/Login?ErrorMessage=Error linking external login");
        }

        // ═══════════════════════════════════════════════════════════════════════════════
        // KROK 4: Dodaj custom claims dla nowego użytkownika
        // ═══════════════════════════════════════════════════════════════════════════════

        await _userClaimsService.UpdateUserClaimsAsync(existingUser, fullName, email);

        // Zaloguj użytkownika
        await _signInManager.SignInAsync(user, isPersistent: false);
        _logger.LogInformation("New user created and logged in with {Provider}: {Email}", info.LoginProvider, email);

        return LocalRedirect(returnUrl);
    }

    /// <summary>
    /// Wylogowanie użytkownika.
    /// </summary>
    [HttpGet]
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