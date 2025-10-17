namespace KERP.Application.Validation;

/// <summary>
/// Definiuje walidatora dla requestu (Command lub Query).
/// </summary>
/// <typeparam name="TRequest">Typ requestu do walidacji (zazwyczaj Command).</typeparam>
public interface IValidator<in TRequest>
{
    /// <summary>
    /// Asynchronicznie waliduje request.
    /// </summary>
    /// <param name="request">Request do walidacji (Command lub Query).</param>
    /// <param name="serviceProvider">Dostawca usług z DI (dla walidatorów wymagających dostępu do bazy).</param>
    /// <param name="cancellationToken">Token anulowania operacji.</param>
    /// <returns>Rezultat walidacji zawierający listę błędów (jeśli są).</returns>
    Task<ValidationResult> ValidateAsync(
        TRequest request,
        IServiceProvider serviceProvider, // ← NOWY PARAMETR
        CancellationToken cancellationToken);
}

