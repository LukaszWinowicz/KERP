using KERP.Application.Common.Abstractions.CQRS;
using KERP.Application.Common.Models;
using KERP.Application.Services;
using KERP.Domain.Aggregates.MassUpdate.PurchaseOrder;

namespace KERP.Application.Common.Abstractions.MassUpdate.PurchaseOrder.ReceiptDate;

/// <summary>
/// Handler dla polecenia UpdateReceiptDateCommand. Odpowiada za wykonanie logiki biznesowej.
/// </summary>
public class UpdateReceiptDateCommandHandler : ICommandHandler<UpdateReceiptDateCommand, Result>
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateReceiptDateCommandHandler(
        IAppDbContext dbContext,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(
        UpdateReceiptDateCommand command,
        CancellationToken cancellationToken)
    {
        {

            // ═══════════════════════════════════════════════════════════════════════════════
            // KROK 1: Tworzenie encji domenowej przy użyciu Factory Method
            // ═══════════════════════════════════════════════════════════════════════════════

            // Walidacja zapewniła że UserId i FactoryId nie są null,
            // więc możemy bezpiecznie użyć .Value bez sprawdzania
            var receiptDateUpdate = ReceiptDateUpdate.Create(
                purchaseOrderNumber: command.PurchaseOrderNumber,
                lineNumber: command.LineNumber,
                sequence: command.Sequence,
                receiptDate: command.ReceiptDate,
                dateType: command.DateType,
                userId: _currentUserService.UserId!, // Walidacja zapewnia że nie jest null
                factoryId: _currentUserService.FactoryId!.Value); // Walidacja zapewnia że nie jest null

            // ═══════════════════════════════════════════════════════════════════════════════
            // KROK 2: Dodanie encji do Change Tracker
            // ═══════════════════════════════════════════════════════════════════════════════

            // Entity Framework oznacza encję jako "Added"
            // Fizyczny zapis do bazy nastąpi w SaveChangesAsync()
            _dbContext.ReceiptDateUpdates.Add(receiptDateUpdate);

            // ═══════════════════════════════════════════════════════════════════════════════
            // KROK 3: Zapis do bazy danych
            // ═══════════════════════════════════════════════════════════════════════════════

            // TransactionBehavior opakowuje to w transakcję bazodanową:
            // - Result.IsSuccess → COMMIT
            // - Result.IsFailure → ROLLBACK
            // - Wyjątek → ROLLBACK + ExceptionHandlingBehavior
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // ═══════════════════════════════════════════════════════════════════════════════
            // KROK 4: Zwrócenie wyniku sukcesu
            // ═══════════════════════════════════════════════════════════════════════════════

            return Result.Success();
        }
    }
}