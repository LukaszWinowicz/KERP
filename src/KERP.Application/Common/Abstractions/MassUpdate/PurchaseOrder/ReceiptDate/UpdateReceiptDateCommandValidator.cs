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
            // ═══════════════════════════════════════════════════════════════════════════
            // KROK 1: Walidacja kontekstu użytkownika (NOWE!)
            // ═══════════════════════════════════════════════════════════════════════════
            // Automatycznie dodaje UserFactoryValidator i FactoryActiveValidator,
            // ponieważ UpdateReceiptDateCommand implementuje IRequireFactoryValidation
            .WithFactoryValidationIfRequired()
            // ═══════════════════════════════════════════════════════════════════════════
            // KROK 2: Walidacja pól wejściowych (istniejące)
            // ═══════════════════════════════════════════════════════════════════════════
            .WithNotEmpty(cmd => cmd.PurchaseOrderNumber, "PurchaseOrderNumber")
            .WithNotNull(cmd => cmd.ReceiptDate, "ReceiptDate")
            .WithFutureDate(cmd => cmd.ReceiptDate, "ReceiptDate")
            .WithMinValue(cmd => cmd.LineNumber, 10, "LineNumber")
            .WithMinValue(cmd => cmd.Sequence, 1, "Sequence")
            .Build();
    }

    public async Task<ValidationResult> ValidateAsync(
        UpdateReceiptDateCommand request,
        IServiceProvider serviceProvider, // ← Teraz dostajemy ServiceProvider z ValidationBehavior
        CancellationToken cancellationToken)
    {
        // Tworzymy kontekst walidacji z wszystkimi potrzebnymi danymi
        var context = new ValidationContext<UpdateReceiptDateCommand>(
            itemToValidate: request,
            serviceProvider: serviceProvider, // ← Przekazujemy do walidatorów
            cancellationToken: cancellationToken);

        // Uruchamiamy łańcuch walidacji
        await _validationHandler.HandleAsync(context);

        // Zwracamy wynik (lista błędów lub pusta lista jeśli OK)
        return new ValidationResult(context.Errors);
    }
}