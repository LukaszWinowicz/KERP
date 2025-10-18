using KERP.Application.Validation;
using KERP.Application.Validation.Chain;

namespace KERP.Application.Common.Abstractions.MassUpdate.PurchaseOrder.ReceiptDate;

public class UpdateReceiptDateBatchCommandValidator : IValidator<UpdateReceiptDateBatchCommand>
{
    private readonly ValidationHandler<UpdateReceiptDateBatchCommand> _userContextValidator;
    private readonly ValidationHandler<ReceiptDateUpdateItem> _itemValidator;

    public UpdateReceiptDateBatchCommandValidator()
    {
        // ETAP 1: Budujemy łańcuch walidacji dla kontekstu użytkownika (cała komenda)
        _userContextValidator = new ValidationChainBuilder<UpdateReceiptDateBatchCommand>()
            .WithFactoryValidationIfRequired() // Automatycznie dodaje UserFactoryValidator i FactoryActiveValidator
            .Build();

        // ETAP 2: Budujemy łańcuch walidacji dla pojedynczego wiersza (item)
        _itemValidator = new ValidationChainBuilder<ReceiptDateUpdateItem>()
            .WithNotEmpty(item => item.PurchaseOrderNumber, "PurchaseOrderNumber")
            .WithNotNull(item => item.ReceiptDate, "ReceiptDate")
            .WithFutureDate(item => item.ReceiptDate, "ReceiptDate")
            .WithMinValue(item => item.LineNumber, 10, "LineNumber")
            .WithMinValue(item => item.Sequence, 1, "Sequence")
            .Build();
    }

    public async Task<ValidationResult> ValidateAsync(
        UpdateReceiptDateBatchCommand request, 
        IServiceProvider serviceProvider, 
        CancellationToken cancellationToken)
    {
        // ═══════════════════════════════════════════════════════════════
        // KROK 0: Sprawdź czy lista nie jest pusta
        // ═══════════════════════════════════════════════════════════════
        if (request.Items == null || !request.Items.Any())
        {
            return new ValidationResult(new[]
            {
            new ValidationError(
                PropertyName: "Items", 
                ErrorMessage: "Dodaj przynajmniej jeden wiersz do zapisania.")
            });
        }

        // ═══════════════════════════════════════════════════════════════════════════════
        // KROK 1: WALIDACJA GLOBALNA (UŻYTKOWNIK I FABRYKA)
        // ═══════════════════════════════════════════════════════════════════════════════
        var userContext = new ValidationContext<UpdateReceiptDateBatchCommand>(request, serviceProvider, cancellationToken);
        await _userContextValidator.HandleAsync(userContext);

        if (userContext.Errors.Any())
        {
            // Jeśli walidacja użytkownika nie powiodła się, natychmiast zwracamy błąd.
            // Nie przechodzimy do walidacji wierszy.
            return new ValidationResult(userContext.Errors);
        }

        // ═══════════════════════════════════════════════════════════════════════════════
        // KROK 2: WALIDACJA LOKALNA (PER-WIERSZ)
        // ═══════════════════════════════════════════════════════════════════════════════

        var allItemErrors = new List<ValidationError>();

        for (int i = 0; i < request.Items.Count; i++)
        {
            var item = request.Items[i];
            var itemContext = new ValidationContext<ReceiptDateUpdateItem>(item, serviceProvider, cancellationToken);
            await _itemValidator.HandleAsync(itemContext);

            if (itemContext.Errors.Any())
            {
                // Modyfikujemy błędy, aby zawierały informację o indeksie wiersza.
                // To pozwoli frontendowi przypisać błąd do odpowiedniego pola.
                var indexedErrors = itemContext.Errors
                    .Select(e => new ValidationError(
                        PropertyName: $"Row[{i}].{e.PropertyName}",
                        ErrorMessage: e.ErrorMessage
                    ));

                allItemErrors.AddRange(indexedErrors);
            }
        }

        return new ValidationResult(allItemErrors);
    }
}
