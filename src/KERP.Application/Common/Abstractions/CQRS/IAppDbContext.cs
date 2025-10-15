using KERP.Domain.Aggregates.MassUpdate.PurchaseOrder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace KERP.Application.Common.Abstractions.CQRS;

public interface IAppDbContext
{
    // Udostępniamy tylko te DbSet'y, których potrzebuje warstwa aplikacji
    DbSet<ReceiptDateUpdate> ReceiptDateUpdates { get; }

    DatabaseFacade Database { get; }
    // Udostępniamy też metodę do zapisu, która będzie używana przez nasz IUnitOfWork
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
