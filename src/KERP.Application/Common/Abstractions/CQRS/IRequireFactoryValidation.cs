namespace KERP.Application.Common.Abstractions.CQRS;

/// <summary>
/// Marker interface dla requestów (Command/Query), które wymagają walidacji kontekstu fabryki użytkownika.
/// Jeśli request implementuje ten interfejs, ValidationBehavior automatycznie uruchomi:
/// - UserFactoryValidator (sprawdza czy FactoryId z cookie zgadza się z bazą danych)
/// - FactoryActiveValidator (sprawdza czy fabryka jest aktywna)
/// </summary>
/// <remarks>
/// Ten interfejs rozwiązuje problem "stale cookie vs dynamiczna baza danych":
/// 
/// PROBLEM:
/// - User loguje się → FactoryId zapisane w cookie (ważne 7 dni)
/// - Admin zmienia FactoryId w bazie → cookie nadal ma starą wartość
/// - User może operować na danych nieprzypisanej fabryki
/// 
/// ROZWIĄZANIE:
/// - Request implementuje IRequireFactoryValidation
/// - ValidationBehavior automatycznie sprawdza aktualny stan w bazie
/// - Jeśli niespójność → Result.Failure przed wykonaniem handlera
/// 
/// PRZYKŁAD UŻYCIA:
/// <code>
/// public record UpdateReceiptDateCommand(...) 
///     : ICommand&lt;Result&gt;, 
///       IRequireFactoryValidation  ← Dodaj ten interfejs
/// {
///     // Walidacja fabryki wykona się automatycznie!
/// }
/// </code>
/// </remarks>
public interface IRequireFactoryValidation
{
    // Marker interface - nie wymaga implementacji
}
