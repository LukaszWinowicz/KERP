namespace KERP.Domain.Common;

/// <summary>
/// Reprezentuje korzeń agregatu w modelu DDD[cite: 9].
/// Agregat jest granicą spójności transakcyjnej dla grupy powiązanych obiektów.
/// </summary>
/// <typeparam name="TId">Typ identyfikatora agregatu.</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId> where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// Pobiera kolekcję zdarzeń domenowych, które wystąpiły w agregacie.
    /// Kolekcja jest tylko do odczytu.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Inicjalizuje nową instancję klasy <see cref="AggregateRoot{TId}"/>.
    /// </summary>
    /// <param name="id">Identyfikator agregatu.</param>
    protected AggregateRoot(TId id) : base(id)
    {
    }

    /// <summary>
    /// Chroniony konstruktor bezparametrowy dla ORM.
    /// </summary>
    protected AggregateRoot() { }

    /// <summary>
    /// Dodaje zdarzenie domenowe do agregatu.
    /// Zdarzenia te powinny być następnie rozdysponowane (dispatched) po pomyślnym zapisaniu zmian.
    /// </summary>
    /// <param name="domainEvent">Zdarzenie domenowe do dodania.</param>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Czyści listę zdarzeń domenowych.
    /// Powinno być wywołane po tym, jak zdarzenia zostaną pomyślnie rozdysponowane.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}