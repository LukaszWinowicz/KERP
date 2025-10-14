using KERP.Domain.Aggregates.Factory;

namespace KERP.Application.Common.Abstractions.Repositories;

public interface IFactoryRepository
{
    /// <summary>
    /// Pobiera fabrykę po ID.
    /// </summary>
    /// <returns></returns>
    Task<Factory?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Pobiera wszystkie aktywne fabryki.
    /// </summary>
    Task<IReadOnlyList<Factory>> GetActiveFactoriesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Sprawdza czy fabryka istnieje i jest aktywna.
    /// </summary>
    Task<bool> ExistsAndIsActiveAsync(int id, CancellationToken cancellationToken = default);
}
