using KERP.Application.Common.Abstractions.Repositories;
using KERP.Domain.Aggregates.Factory;
using Microsoft.EntityFrameworkCore;

namespace KERP.Infrastructure.Persistence.Repositories;

public class FactoryRepository : IFactoryRepository
{
    private readonly AppDbContext _context;

    public FactoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Factory?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Factories
           .AsNoTracking()
           .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }
    public async Task<IReadOnlyList<Factory>> GetActiveFactoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Factories
            .AsNoTracking()
            .Where(f => f.IsActive)
            .OrderBy(f => f.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAndIsActiveAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Factories
            .AsNoTracking()
            .AnyAsync(f => f.Id == id && f.IsActive, cancellationToken);
    }
}
