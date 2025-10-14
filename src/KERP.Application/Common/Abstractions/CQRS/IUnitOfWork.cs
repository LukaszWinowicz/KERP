namespace KERP.Application.Common.Abstractions.CQRS;

/// <summary>
/// Definiuje kontrakt dla wzorca Unit of Work, reprezentującego pojedynczą transakcję biznesową.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Asynchronicznie zapisuje wszystkie zmiany wprowadzone w ramach tej jednostki pracy do bazy danych.
    /// </summary>
    /// <param name="cancellationToken">Token do anulowania operacji.</param>
    /// <returns>Liczba obiektów, których stan został zmieniony w bazie danych.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
