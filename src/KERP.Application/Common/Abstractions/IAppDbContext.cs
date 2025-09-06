using Microsoft.EntityFrameworkCore.Infrastructure;

namespace KERP.Application.Common.Abstractions;

public interface IAppDbContext
{
    // Udostępniamy tylko te DbSet'y, których potrzebuje warstwa aplikacji


    DatabaseFacade Database { get; }
    // Udostępniamy też metodę do zapisu, która będzie używana przez nasz IUnitOfWork
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
