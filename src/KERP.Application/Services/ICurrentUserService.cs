namespace KERP.Application.Services;

/// <summary>
/// Definiuje serwis dostarczający informacje o aktualnie zalogowanym użytkowniku.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Pobiera unikalny identyfikator zalogowanego użytkownika.
    /// Może być null, jeśli użytkownik jest anonimowy.
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// Pobiera identyfikator fabryki, w kontekście której działa użytkownik.
    /// Może być null, jeśli kontekst fabryki nie jest ustawiony.
    /// </summary>
    int? FactoryId { get; }
}