using KERP.Application.Common.Abstractions.CQRS;
using KERP.Application.Common.Models;
using KERP.Application.Services;
using KERP.Domain.Aggregates.MassUpdate.PurchaseOrder;

namespace KERP.Application.Common.Abstractions.MassUpdate.PurchaseOrder.ReceiptDate;

/// <summary>
/// Handler dla polecenia UpdateReceiptDateBatchCommand. Odpowiada za wykonanie logiki biznesowej.
/// </summary>
public class UpdateReceiptDateBatchCommandHandler : ICommandHandler<UpdateReceiptDateBatchCommand, Result>
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateReceiptDateBatchCommandHandler(
        IAppDbContext dbContext,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(
        UpdateReceiptDateBatchCommand command,
        CancellationToken cancellationToken)
    {
        // Walidacja zapewniła, że UserId i FactoryId nie są null,
        // więc możemy bezpiecznie użyć operatora !.
        var userId = _currentUserService.UserId!;
        var factoryId = _currentUserService.FactoryId!.Value;

        // Przechodzimy przez każdy wiersz z komendy i tworzymy dla niego encję domenową
        foreach (var item in command.Items)
        {
            var receiptDateUpdate = ReceiptDateUpdate.Create(
                purchaseOrderNumber: item.PurchaseOrderNumber,
                lineNumber: item.LineNumber,
                sequence: item.Sequence,
                receiptDate: item.ReceiptDate,
                dateType: command.DateType, // Używamy wspólnego DateType dla całej paczki
                userId: userId,
                factoryId: factoryId);

            // Dodajemy encję do Change Trackera w EF Core
            _dbContext.ReceiptDateUpdates.Add(receiptDateUpdate);
        }

        // Zapisujemy wszystkie zmiany w jednej transakcji
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Zwracamy wynik sukcesu
        return Result.Success();
    }
}