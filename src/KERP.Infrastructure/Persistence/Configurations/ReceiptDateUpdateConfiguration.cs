using KERP.Domain.Aggregates.MassUpdate.PurchaseOrder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KERP.Infrastructure.Persistence.Configurations;

public class ReceiptDateUpdateConfiguration : IEntityTypeConfiguration<ReceiptDateUpdate>
{
    public void Configure(EntityTypeBuilder<ReceiptDateUpdate> builder)
    {
        // Definiujemy nazwę tabeli i schemat, aby utrzymać porządek w bazie danych
        builder.ToTable("MassUpdate_PurchaseOrder_ReceiptDate", "f241");

        // Klucz główny będzie auto-inkrementowany (domyślne zachowanie dla int)
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PurchaseOrderNumber)
            .IsRequired()
            .HasMaxLength(9)
            .IsFixedLength(); // Wymusza stałą długość 9 znaków

        builder.Property(x => x.LineNumber)
            .IsRequired();

        builder.Property(x => x.Sequence)
            .IsRequired();

        // ReceiptDate jest teraz wymagany
        builder.Property(x => x.ReceiptDate)
            .IsRequired();

        builder.Property(x => x.DateType)
            .IsRequired();

        builder.Property(x => x.UserId)
            .IsRequired();

        // FactoryId jest teraz wymagany
        builder.Property(x => x.FactoryId)
            .IsRequired();

        builder.Property(x => x.AddedDate)
            .IsRequired();

        // IsGenerated jest wymagane i ma wartość domyślną 'false'
        builder.Property(x => x.IsGenerated)
            .IsRequired()
            .HasDefaultValue(false);

        // GeneratedDate może być nullem
        builder.Property(x => x.GeneratedDate)
            .IsRequired(false);
    }
}
