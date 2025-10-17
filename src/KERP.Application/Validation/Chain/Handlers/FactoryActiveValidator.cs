using KERP.Application.Common.Abstractions.Repositories;
using KERP.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace KERP.Application.Validation.Chain.Handlers;

/// <summary>
/// Walidator sprawdzający czy fabryka przypisana do użytkownika jest aktywna.
/// </summary>
/// <remarks>
/// Ten walidator jest automatycznie dodawany przez ValidationChainBuilder
/// dla requestów implementujących <see cref="IRequireFactoryValidation"/>.
/// 
/// <para><b>UWAGA:</b></para>
/// Ten walidator MUSI być uruchamiany PO <see cref="UserFactoryValidator{T}"/>,
/// ponieważ zakłada że FactoryId użytkownika zostało już zweryfikowane.
/// </remarks>
public class FactoryActiveValidator<T> : ValidationHandler<T>
{
    private readonly string _fieldName;

    public FactoryActiveValidator(string fieldName = "Factory")
    {
        _fieldName = fieldName;
    }

    protected override async Task ValidateAsync(ValidationContext<T> context)
    {
        // ═══════════════════════════════════════════════════════════════════════════════
        // KROK 1: Pobierz wymagane serwisy z DI
        // ═══════════════════════════════════════════════════════════════════════════════

        var currentUserService = context.ServiceProvider
            .GetRequiredService<ICurrentUserService>();

        var factoryRepository = context.ServiceProvider
            .GetRequiredService<IFactoryRepository>();

        // ═══════════════════════════════════════════════════════════════════════════════
        // KROK 2: Sprawdź czy użytkownik ma FactoryId
        // ═══════════════════════════════════════════════════════════════════════════════

        // To jest dodatkowa ochrona - UserFactoryValidator powinien był to już sprawdzić,
        // ale lepiej być ostrożnym (defense in depth)
        if (!currentUserService.FactoryId.HasValue)
        {
            // Nie dodajemy błędu - UserFactoryValidator już to zrobił
            // Po prostu przerywamy ten walidator
            return;
        }

        // ═══════════════════════════════════════════════════════════════════════════════
        // KROK 2: Sprawdź czy fabryka istnieje i jest aktywna
        // ═══════════════════════════════════════════════════════════════════════════════

        var isFactoryActive = await factoryRepository.ExistsAndIsActiveAsync(
            currentUserService.FactoryId.Value,
            context.CancellationToken);

        if (!isFactoryActive)
        {
            // Używamy FactoryName z CurrentUserService dla lepszego UX
            // (użytkownik widzi nazwę, nie ID)
            var factoryName = currentUserService.FactoryName ?? $"Fabryka {currentUserService.FactoryId.Value}";

            context.Errors.Add(new ValidationError(
                PropertyName: _fieldName,
                ErrorMessage: $"Fabryka {factoryName} jest nieaktywna lub nie istnieje. Skontaktuj się z administratorem."
            ));
        }
    }
}
