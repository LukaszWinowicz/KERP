using KERP.Domain.Aggregates.Factory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KERP.Infrastructure.Persistence.Configurations;

public class FactoryConfiguration : IEntityTypeConfiguration<Factory>
{
    public void Configure(EntityTypeBuilder<Factory> builder)
    {
        builder.ToTable("Factories", "cmn"); // cmn.Factories

        // KLUCZOWE: Id NIE jest auto-increment!
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id)
            .ValueGeneratedNever(); // ← To mówi EF: NIE generuj wartości!

        builder.Property(f => f.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(f => f.Name)
            .IsUnique();

        // SEED DATA - inicjalne fabryki
        builder.HasData(
            Factory.Create(241, "Stargard", isActive: true),
            Factory.Create(276, "Ottawa", isActive: true),
            Factory.Create(260, "Shanghai", isActive: true)
        );
    }
}
