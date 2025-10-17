using Microsoft.Extensions.DependencyInjection;

namespace KERP.Application.Validation.Chain;

/// <summary>
/// Generyczny kontekst walidacji, który przechowuje element do walidacji oraz dostawcę usług.
/// </summary>
/// <typeparam name="T">Typ obiektu poddawanego walidacji.</typeparam>
public record ValidationContext<T>
{
    /// <summary>
    /// Obiekt, który jest aktualnie walidowany (Command lub Query).
    /// </summary>
    public T ItemToValidate { get; init; }

    /// <summary>
    /// Dostawca usługi (IServiceProvider) do rozwiązywania zależności.
    /// Umożliwia walidatorom dostęp do serwisów z DI (np. UserManager, Repositories).
    /// </summary>
    public IServiceProvider ServiceProvider { get; init; }

    /// <summary>
    /// Token anulowania operacji asynchronicznych.
    /// Przekazywany do walidatorów wykonujących zapytania do bazy danych.
    /// </summary>
    public CancellationToken CancellationToken { get; init; }

    /// <summary>
    /// Lista błędów walidacji, które mogą być dodane podczas procesu walidacji.
    /// </summary>
    public List<ValidationError> Errors { get; } = new List<ValidationError>();

    /// <summary>
    /// Flaga wskazująca, czy walidacja powinna zostać przerwana.
    /// </summary>
    public bool ShouldStop { get; private set; }

    /// <summary>
    /// Konstruktor tworzący nowy kontekst walidacji.
    /// </summary>
    /// <param name="itemToValidate">Obiekt do walidacji.</param>
    /// <param name="serviceProvider">Dostawca usług z DI.</param>
    /// <param name="cancellationToken">Token anulowania operacji.</param>
    public ValidationContext(
        T itemToValidate,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        ItemToValidate = itemToValidate;
        ServiceProvider = serviceProvider;
        CancellationToken = cancellationToken;
    }

    /// <summary>
    /// Zatrzymuje proces walidacji.
    /// Kolejne walidatory w łańcuchu nie zostaną wykonane.
    /// </summary>
    public void StopValidation()
    {
        ShouldStop = true;
    }
}