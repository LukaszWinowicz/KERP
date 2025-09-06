using System.Reflection;
using KERP.Application.Common.Abstractions;
using KERP.Domain.Aggregates.User;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace KERP.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<ApplicationUser>, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Ta linijka jest teraz jeszcze ważniejsza.
        // Najpierw wołamy base.OnModelCreating, aby ASP.NET Identity
        // mogło skonfigurować swoje własne tabele (Users, Roles, Claims itp.).
        base.OnModelCreating(modelBuilder);

        // Następnie stosujemy nasze własne, niestandardowe konfiguracje.
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
