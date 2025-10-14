using KERP.Domain.Common;

namespace KERP.Domain.Aggregates.Factory;

/// <summary>
/// Reprezentuje fabrykę w systemie ERP.
/// Factory ID jest unikalnym identyfikatorem biznesowym (np. 241, 260, 273).
/// </summary>
public sealed class Factory : AggregateRoot<int>
{
    /// <summary>
    /// Nazwa fabryki (np. "Stargard", "Ottawa", "Shanghai")
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Czy fabryka jest aktywna w systemie
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Konstruktor dla EF Core (protected dla ORM)
    /// </summary>
    private Factory() : base()
    {
        Name = string.Empty;
        IsActive = true;
    }

    /// <summary>
    /// Konstruktor prywatny - użyj Factory.Create()
    /// </summary>
    private Factory(int id, string name, bool isActive = true) : base(id)
    {
        Name = name;
        IsActive = isActive;
    }

    /// <summary>
    /// Tworzy nową fabrykę z konkretnym ID biznesowym.
    /// </summary>
    /// <param name="id">Unikalny identyfikator fabryki (np. 241, 260, 273)</param>
    /// <param name="name">Nazwa fabryki</param>
    /// <param name="isActive">Czy fabryka jest aktywna</param>
    public static Factory Create(int id, string name, bool isActive = true)
    {
        if (id <= 0)
            throw new ArgumentException("Factory ID must be greater than 0", nameof(id));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Factory name cannot be empty", nameof(name));

        return new Factory(id, name, isActive);
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Factory name cannot be empty", nameof(name));

        Name = name;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
