using KERP.Domain.Common;

namespace KERP.Domain.Aggregates.MassUpdate.PurchaseOrder;

/// <summary>
/// Reprezentuje pojedynczą operację zmiany daty odbioru dla linii zamówienia.
/// Jest to korzeń agregatu (Aggregate Root) w kontekście tej operacji.
/// </summary>
public class ReceiptDateUpdate : AggregateRoot<int>
{
    public string PurchaseOrderNumber { get; private set; }
    public int LineNumber { get; private set; }
    public int Sequence { get; private set; }
    public DateTime? ReceiptDate { get; private set; }
    public DateType DateType { get; private set; }
    public string UserId { get; private set; }
    public int? FactoryId { get; private set; }
    public DateTime AddedDate { get; private set; }
    public bool IsGenerated { get; private set; }
    public DateTime? GeneratedDate { get; private set; }

    /// <summary>
    /// Prywatny konstruktor na potrzeby Entity Framework Core.
    /// </summary>
    private ReceiptDateUpdate()
    {
        // Wymagane przez EF Core do materializacji encji
        PurchaseOrderNumber = string.Empty;
        UserId = string.Empty;
    }

    /// <summary>
    /// Prywatny konstruktor do tworzenia instancji poprzez metodę fabrykującą Create.
    /// </summary>
    private ReceiptDateUpdate(
        string purchaseOrderNumber,
        int lineNumber,
        int sequence,
        DateTime? receiptDate,
        DateType dateType,
        string userId,
        int? factoryId)
    {
        PurchaseOrderNumber = purchaseOrderNumber;
        LineNumber = lineNumber;
        Sequence = sequence;
        ReceiptDate = receiptDate;
        DateType = dateType;
        UserId = userId;
        FactoryId = factoryId;
        AddedDate = DateTime.UtcNow;
        IsGenerated = false; // Domyślna wartość
        GeneratedDate = null; // Domyślna wartość
    }

    /// <summary>
    /// Metoda fabrykująca (factory method) do tworzenia nowej instancji ReceiptDateUpdate.
    /// Zapewnia, że obiekt jest tworzony w spójnym i poprawnym stanie.
    /// </summary>
    public static ReceiptDateUpdate Create(
        string purchaseOrderNumber,
        int lineNumber,
        int sequence,
        DateTime? receiptDate,
        DateType dateType,
        string userId,
        int? factoryId)
    {
        // Tutaj można dodać niezmienniki domenowe, np. sprawdzanie czy UserId nie jest pusty.
        // Jednak zgodnie z ustaleniami, główna walidacja odbędzie się w warstwie Application.

        return new ReceiptDateUpdate(
            purchaseOrderNumber,
            lineNumber,
            sequence,
            receiptDate,
            dateType,
            userId,
            factoryId);
    }
}