using KERP.Application.Common.Abstractions.CQRS;
using KERP.Application.Common.Models;
using KERP.Domain.Aggregates.MassUpdate.PurchaseOrder;

namespace KERP.Application.Common.Abstractions.MassUpdate.PurchaseOrder.ReceiptDate;

/// <summary>
/// Polecenie (Command) reprezentujące żadanie masowej aktualizacji Receipt Date.
/// Implementuje IRequireFactoryValidation, co automatycznie włącza walidację kontekstu użytkownika.
/// </summary>
/// <param name="Items"></param>
/// <param name="DateType"></param>
public record UpdateReceiptDateBatchCommand(
    IReadOnlyList<ReceiptDateUpdateItem> Items,
    DateType DateType) 
    : ICommand<Result>,
      IRequireFactoryValidation;

/// <summary>
/// Reprezentuje pojedynczy wiersz w masowej aktualizacji Receipt Date.
/// </summary>
public record ReceiptDateUpdateItem(
    string PurchaseOrderNumber,
    int LineNumber,
    int Sequence,
    DateTime? ReceiptDate);