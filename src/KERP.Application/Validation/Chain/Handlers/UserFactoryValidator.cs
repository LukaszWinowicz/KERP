using KERP.Application.Services;
using KERP.Domain.Aggregates.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace KERP.Application.Validation.Chain.Handlers;

/// <summary>
/// Walidator sprawdzający czy FactoryId użytkownika z cookie (session)
/// zgadza się z aktualnym FactoryId w bazie danych.
/// </summary>
/// <remarks>
/// Ten walidator jest automatycznie dodawany przez ValidationChainBuilder
/// dla requestów implementujących <see cref="IRequireFactoryValidation"/>.
/// </remarks>
/// <typeparam name="T"></typeparam>
public class UserFactoryValidator<T> : ValidationHandler<T>
{
    private readonly string _fieldName;

    public UserFactoryValidator(string fieldName = "UserFactory")
    {
        _fieldName = fieldName;
    }

    protected override async Task ValidateAsync(ValidationContext<T> context)
    {
        // ═══════════════════════════════════════════════════════════════════════════════
        // KROK 1: Pobierz wymagane serwisy z DI
        // ═══════════════════════════════════════════════════════════════════════════════

        // IServiceProvider jest przekazywany w ValidationContext, aby walidatory
        // mogły korzystać z serwisów zarejestrowanych w DI (np. UserManager, DbContext)
        var currentUserService = context.ServiceProvider
            .GetRequiredService<ICurrentUserService>();

        var userManager = context.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        // ═══════════════════════════════════════════════════════════════════════════════
        // KROK 2: Walidacja podstawowa - czy user ma FactoryId w cookie
        // ═══════════════════════════════════════════════════════════════════════════════

        // To jest szybki early return - jeśli cookie nie ma FactoryId,
        // nie ma sensu odpytywać bazy danych
        if (!currentUserService.FactoryId.HasValue)
        {
            context.Errors.Add(new ValidationError(
                PropertyName: _fieldName,
                ErrorMessage: "Użytkownik nie ma przypisanej fabryki w sesji. Zaloguj się ponownie."
            ));
            return;
        }

        // Analogicznie dla UserId - jeśli nie ma, nie możemy sprawdzić w bazie
        if (string.IsNullOrEmpty(currentUserService.UserId))
        {
            context.Errors.Add(new ValidationError(
                PropertyName: _fieldName,
                ErrorMessage: "Nie można zweryfikować użytkownika. Zaloguj się ponowanie."
            ));
            return;
        }

        // ═══════════════════════════════════════════════════════════════════════════════
        // KROK 3: Pobierz aktualny stan użytkownika z bazy danych
        // ═══════════════════════════════════════════════════════════════════════════════

        // WAŻNE: To jest query do bazy danych!
        // UserManager używa Entity Framework pod spodem
        var currentUser = await userManager.FindByIdAsync(currentUserService.UserId);

        if (currentUser == null)
        {
            context.Errors.Add(new ValidationError(
                PropertyName: _fieldName,
                ErrorMessage: "Użytkownik nie został znaleziony w systemie. Skontaktuj się z administratorem."
            ));
            return;
        }

        // ═══════════════════════════════════════════════════════════════════════════════
        // KROK 4: Sprawdź czy użytkownik NADAL ma przypisaną fabrykę w bazie
        // ═══════════════════════════════════════════════════════════════════════════════

        // Scenariusz: Admin usunął FactoryId (UPDATE ... SET FactoryId = NULL)
        if (!currentUser.FactoryId.HasValue)
        {
            context.Errors.Add(new ValidationError(
                PropertyName: _fieldName,
                ErrorMessage: "Twoje konto nie ma przypisanej fabryki. Skontaktuj się z administratorem i zaloguj się ponownie."
            ));
            return;
        }

        // ═══════════════════════════════════════════════════════════════════════════════
        // KROK 5: Porównaj FactoryId z cookie z FactoryId z bazy danych
        // ═══════════════════════════════════════════════════════════════════════════════

        // To jest kluczowa walidacja! 
        // Cookie może być "stare" (max 7 dni), baza jest źródłem prawdy

        if (currentUser.FactoryId.Value != currentUserService.FactoryId.Value)
        {
            context.Errors.Add(new ValidationError(
                PropertyName: _fieldName,
                ErrorMessage: $"Twoja fabryka została zmieniona z {currentUserService.FactoryName} na inną. Zaloguj się ponownie aby kontynuować."
            ));
        }
        return;
    }
}
