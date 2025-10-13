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

            // Zaktualizuj claims dla istniejącego użytkownika
            await UpdateUserClaimsAsync(user, fullName, email);

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
                await UpdateUserClaimsAsync(existingUser, fullName, email);
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

        await UpdateUserClaimsAsync(user, fullName, email);

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

    /// <summary>
    /// Aktualizuje custom claims dla użytkownika.
    /// Ta metoda zarządza wszystkimi naszymi własnymi claims.
    /// </summary>
    private async Task UpdateUserClaimsAsync(ApplicationUser user, string fullName, string email)
    {
        // Pobranie istniejących claims użytkownika.
        var existingClaims = await _userManager.GetClaimsAsync(user);

        // Lista typów claims, które chcemy zaktualizować
        var claimTypesToUpdate = new[]
        {
            "GivenUsername",
            "GivenEmail",
            "FactoryId",
            "FactoryName"
        };

        // Usunięcie starych wersji tych claims (jeśli istnieją)
        var claimsToRemove = existingClaims
            .Where(c => claimTypesToUpdate.Contains(c.Type))
            .ToList();

        if (claimsToRemove.Any())
        {
            var removeResult = await _userManager.RemoveClaimsAsync(user, claimsToRemove);
            if (!removeResult.Succeeded)
            {
                _logger.LogWarning("Failed to remove old claims for user {UserId}", user.Id);
            }
        }

        // Przygotuj nowe claims
        var newClaims = new List<Claim>
        {
            new Claim("GivenUsername", fullName),
            new Claim("GivenEmail", email)
        };

        // Dodaj FactoryId i FactoryName jeśli użytkownik ma przypisaną fabrykę
        if (user.FactoryId.HasValue)
        {
            newClaims.Add(new Claim("FactoryId", user.FactoryId.Value.ToString()));

            // TODO: W przyszłości pobierać nazwę z bazy/słownika
            // Na razie hardkodujemy mapowanie
            var factoryName = GetFactoryName(user.FactoryId.Value);
            newClaims.Add(new Claim("FactoryName", factoryName));
        }

        // Zapisz nowe claims
        var addResult = await _userManager.AddClaimsAsync(user, newClaims);
        if (addResult.Succeeded)
        {
            _logger.LogInformation("Successfully update claims for user {UserId}", user.Id);
            // WAŻNE: Odśwież sign-in aby nowe claims były widoczne od razu
            await _signInManager.RefreshSignInAsync(user);
        }
        else
        {
            var errors = string.Join(", ", addResult.Errors.Select(e => e.Description));
            _logger.LogError("Failed to add claims for user {UserId}: {Errors}", user.Id, errors);
        }
    }

    /// <summary>
    /// Tymczasowa metoda do mapowania FactoryId na nazwę.
    /// W przyszłości należy to zastąpić serwisem lub repozytorium.
    /// </summary>
    private string GetFactoryName(int factoryId)
    {
        // TODO: Zastąpić rzeczywistym słownikiem lub zapytaniem do bazy
        return factoryId switch
        {
            241 => "Stargard",
            260 => "Ottawa",
            273 => "Shanghai",
            _ => $"Factory {factoryId}"
        };
    }
}