namespace KERP.Application.Validation;

/// <summary>
/// Reprezentuje rezultat walidacji wejściowej.
/// </summary>
public sealed record ValidationResult(IReadOnlyCollection<ValidationError> Errors)
{
    public static ValidationResult Success() => new ValidationResult(Array.Empty<ValidationError>());
}
