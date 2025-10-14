using KERP.Domain.Aggregates.Factory;
using KERP.Domain.Aggregates.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KERP.Infrastructure.Persistence.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.HasOne<Factory>()
            .WithMany()
            .HasForeignKey(u => u.FactoryId)
            .OnDelete(DeleteBehavior.Restrict); // Nie pozwól usunąć Factory jeśli są przypisani userzy
    }
}
