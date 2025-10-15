using KERP.Application.Validation;
using KERP.Application.Validation.Chain;

namespace KERP.Application.Common.Abstractions.MassUpdate.PurchaseOrder.ReceiptDate;

/// <summary>
/// Walidator dla polecenia UpdateReceiptDateCommand.
/// Wykorzystuje istniejący ValidationChainBuilder do zdefiniowania reguł.
/// </summary>
public class UpdateReceiptDateCommandValidator : IValidator<UpdateReceiptDateCommand>
{
    private readonly ValidationHandler<UpdateReceiptDateCommand> _validationHandler;

    public UpdateReceiptDateCommandValidator()
    {
        // Budujemy łańcuch walidacji przy użyciu dostarczonych "klocków"
        // Poprawiono błąd: zamiast nameof(cmd.Property) używamy "Property"
        _validationHandler = new ValidationChainBuilder<UpdateReceiptDateCommand>()
            .WithNotEmpty(cmd => cmd.PurchaseOrderNumber, "PurchaseOrderNumber")
            .WithNotNull(cmd => cmd.ReceiptDate, "ReceiptDate")
            .WithFutureDate(cmd => cmd.ReceiptDate, "ReceiptDate")
            .WithMinValue(cmd => cmd.LineNumber, 10, "LineNumber")
            .WithMinValue(cmd => cmd.Sequence, 1, "Sequence")
            .Build();
    }

    public async Task<ValidationResult> ValidateAsync(UpdateReceiptDateCommand request, CancellationToken cancellationToken)
    {
        var context = new ValidationContext<UpdateReceiptDateCommand>(request, null!); // serviceProvider jest null, bo nie mamy walidacji z bazą danych
        await _validationHandler.HandleAsync(context);
        return new ValidationResult(context.Errors);
    }
}