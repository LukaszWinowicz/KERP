namespace KERP.Application.Validation.Chain;

/// <summary>
/// Generyczny kontekst walidacji, który przechowuje element do walidacji oraz dostawcę usług, umożliwiając do repozytoriów w razie potrzeby.
/// </summary>
/// <typeparam name="T">Typ obiektu poddawanego walidacji.</typeparam>
/// <param name="ItemToValidate">Obiekt, który jest aktualnie walidowany.</param>
/// <param name="service">Dostawca usług (IServiceProvider) do rozwiązywania zależności.</param>
public record ValidationContext<T>(T ItemToValidate, IServiceProvider service)
{
    /// <summary>
    /// Lista błędów walidacji, które mogą być dodane podczas procesu walidacji.
    /// </summary>
    public List<ValidationError> Errors { get; } = new List<ValidationError>();

    /// <summary>
    /// Flaga wskazująca, czy walidacja powinna zostać przerwana.
    /// </summary>
    public bool ShouldStop { get; private set; }

    /// <summary>
    /// Zatrzymuje proces walidacji.
    /// </summary>
    public void StopValidation()
    {
        ShouldStop = true;
    }
}