namespace KERP.Domain.Common;

/// <summary>
/// Interfejs-znacznik dla zdarzeń domenowych[cite: 6].
/// Reprezentuje coś, co wydarzyło się w domenie i jest istotne
/// dla innych części systemu lub systemów zewnętrznych.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Pobiera datę i czas wystąpienia zdarzenia (w UTC).
    /// </summary>
    public DateTime DateOccurred { get; }
}
