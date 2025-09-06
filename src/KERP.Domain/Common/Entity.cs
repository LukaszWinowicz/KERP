namespace KERP.Domain.Common;

/// <summary>
/// Bazowa klasa dla wszystkich bytów (Entities) w systemie.
/// Byt jest obiektem definiowanym przez swoją tożsamość (Id), a nie atrybuty.
/// </summary>
/// <typeparam name="TId">Typ identyfikatora bytu.</typeparam>
public abstract class Entity<TId> where TId : notnull
{
    /// <summary>
    /// Unikalny identyfikator bytu.
    /// </summary>
    public TId Id { get; protected set; }

    /// <summary>
    /// Inicjalizuje nową instancję klasy <see cref="Entity{TId}"/>.
    /// </summary>
    /// <param name="id">Identyfikator bytu.</param>
    protected Entity(TId id)
    {
        Id = id;
    }

    /// <summary>
    /// Chroniony konstruktor bezparametrowy wymagany przez niektóre frameworki ORM (np. EF Core).
    /// </summary>
    protected Entity() { }

    /// <summary>
    /// Sprawdza, czy obiekt jest równy bieżącemu bytowi na podstawie jego Id.
    /// </summary>
    /// <param name="obj">Obiekt do porównania.</param>
    /// <returns>True, jeśli obiekty są równe; w przeciwnym razie false.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TId> other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        if (Id.Equals(default) || other.Id.Equals(default))
            return false;

        return Id.Equals(other.Id);
    }

    /// <summary>
    /// Porównuje dwa byty pod kątem równości.
    /// </summary>
    public static bool operator ==(Entity<TId>? a, Entity<TId>? b)
    {
        if (a is null && b is null)
            return true;

        if (a is null || b is null)
            return false;

        return a.Equals(b);
    }

    /// <summary>
    /// Porównuje dwa byty pod kątem nierówności.
    /// </summary>
    public static bool operator !=(Entity<TId>? a, Entity<TId>? b)
    {
        return !(a == b);
    }

    /// <summary>
    /// Zwraca kod skrótu dla bytu.
    /// </summary>
    /// <returns>Kod skrótu.</returns>
    public override int GetHashCode()
    {
        // Użycie nazwy typu zapewnia unikalność hashcode'u między różnymi typami bytów.
        return (GetType().ToString() + Id).GetHashCode();
    }
}