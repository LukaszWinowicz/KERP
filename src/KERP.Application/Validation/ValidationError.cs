namespace KERP.Application.Validation;

/// <summary>
/// Reprezentuje pojedyńczy błąd walidacji wejściowej.
/// </summary>
/// <param name="PropertyName"></param>
/// <param name="ErrorMessage"></param>
public record ValidationError(string PropertyName, string ErrorMessage);