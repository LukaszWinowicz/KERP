using KERP.Application.Common.Abstractions.CQRS;
using KERP.Application.Common.Models;
using KERP.Domain.Aggregates.MassUpdate.PurchaseOrder;

namespace KERP.Application.Common.Abstractions.MassUpdate.PurchaseOrder.ReceiptDate;

/// <summary>
/// Polecenie (Command) reprezentujące żądanie aktualizacji Receipt Date dla linii zamówienia.
/// Implementuje IRequireFactoryValidation, co automatycznie włącza walidację:
/// - Zgodności FactoryId użytkownika (cookie vs baza danych)
/// - Aktywności fabryki użytkownika
/// </summary>
public record UpdateReceiptDateCommand(
    string PurchaseOrderNumber,
    int LineNumber,
    int Sequence,
    DateTime? ReceiptDate,
    DateType DateType) 
    : ICommand<Result>,
      IRequireFactoryValidation;