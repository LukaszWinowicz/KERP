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

    /// <summary>
    /// Pobiera nazwę aktualnie wybranej fabryki.
    /// </summary>
    string? FactoryName { get; }

    /// <summary>
    /// Pobiera nazwę użytkownika (display name).
    /// </summary>
    string? Username { get; }

    /// <summary>
    /// Pobiera email użytkownika.
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Sprawdza czy użytkownik jest zalogowany.
    /// </summary>
    bool IsAuthenticated { get; }
}