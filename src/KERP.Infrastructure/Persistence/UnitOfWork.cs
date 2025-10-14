using KERP.Application.Common.Abstractions.CQRS;

namespace KERP.Infrastructure.Persistence;

/// <summary>
/// Konkretna implementacja wzorca Unit of Work.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Przekazuje wywołanie bezpośrednio do DbContext.
        return _context.SaveChangesAsync(cancellationToken);
    }
}
