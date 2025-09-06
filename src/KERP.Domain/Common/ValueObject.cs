namespace KERP.Domain.Common;

/// <summary>
/// Bazowa klasa dla obiektów wartości (Value Objects).
/// Obiekty wartości są definiowane przez swoje atrybuty i nie posiadają własnej tożsamości.
/// Powinny być niezmienne (immutable).
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    /// <summary>
    /// Metoda, którą muszą zaimplementować klasy pochodne.
    /// Powinna zwracać wszystkie komponenty (właściwości) obiektu wartości,
    /// które uczestniczą w porównywaniu równości.
    /// </summary>
    /// <returns>Kolekcja komponentów do porównania.</returns>
    protected abstract IEnumerable<object> GetEqualityComponents();

    /// <summary>
    /// Porównuje obiekt wartości z innym obiektem.
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
        {
            return false;
        }

        var valueObject = (ValueObject)obj;
        return GetEqualityComponents().SequenceEqual(valueObject.GetEqualityComponents());
    }

    /// <summary>
    /// Porównuje obiekt wartości z innym obiektem wartości.
    /// </summary>
    public bool Equals(ValueObject? other)
    {
        return other is not null && Equals((object)other);
    }

    /// <summary>
    /// Zwraca kod skrótu oparty na komponentach równości.
    /// </summary>
    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Aggregate(1, (current, obj) =>
            {
                return current ^ (obj?.GetHashCode() ?? 0);
            });
    }

    /// <summary>
    /// Porównuje dwa obiekty wartości pod kątem równości.
    /// </summary>
    public static bool operator ==(ValueObject? a, ValueObject? b)
    {
        if (a is null && b is null)
            return true;

        if (a is null || b is null)
            return false;

        return a.Equals(b);
    }

    /// <summary>
    /// Porównuje dwa obiekty wartości pod kątem nierówności.
    /// </summary>
    public static bool operator !=(ValueObject? a, ValueObject? b)
    {
        return !(a == b);
    }
}
