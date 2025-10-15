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

    public UpdateReceiptDateCommandHandler(IAppDbContext dbContext, ICurrentUserService currentUserService, IUnitOfWork unitOfWork)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(UpdateReceiptDateCommand command, CancellationToken cancellationToken)
    {
        // Używamy metody fabrykującej z encji do stworzenia nowego obiektu
        var receiptDateUpdate = ReceiptDateUpdate.Create(
            purchaseOrderNumber: command.PurchaseOrderNumber,
            lineNumber: command.LineNumber,
            sequence: command.Sequence,
            receiptDate: command.ReceiptDate,
            dateType: command.DateType,
            userId: _currentUserService.UserId!, // Zakładamy, że operacja wymaga zalogowanego użytkownika
            factoryId: _currentUserService.FactoryId
        );

        // Dodajemy nową encję do DbContext.
        // UWAGA: To jeszcze nie zapisuje danych w bazie, tylko oznacza obiekt jako "do dodania".
        // Będziemy musieli dodać DbSet<ReceiptDateUpdate> do IAppDbContext
        // _dbContext.ReceiptDateUpdates.Add(receiptDateUpdate);

        // Zapisujemy zmiany w bazie danych.
        // To wywołanie jest kluczowe, aby transakcja (zarządzana przez TransactionBehavior) mogła zatwierdzić zmiany.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new List<RowValidationResult>());
    }
}