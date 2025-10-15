using KERP.Application.Common.Abstractions.CQRS;
using KERP.Application.Common.Models;
using KERP.Domain.Aggregates.MassUpdate.PurchaseOrder;

namespace KERP.Application.Common.Abstractions.MassUpdate.PurchaseOrder.ReceiptDate;

/// <summary>
/// Polecenie (Command) reprezentujące żądanie aktualizacji daty odbioru dla linii zamówienia.
/// Używa rekordu, aby zapewnić niezmienność (immutability) danych wejściowych.
/// </summary>
public record UpdateReceiptDateCommand(
    string PurchaseOrderNumber,
    int LineNumber,
    int Sequence,
    DateTime? ReceiptDate,
    DateType DateType) : ICommand<Result>;