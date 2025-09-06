namespace KERP.Application.Validation.Chain.Handlers;

/// <summary>
/// Sprawdza, czy data (jeśli została podana) jest w przyszłości.
/// Zakłada, że istnienie wartości jest sprawdzane przez inne ogniwo w łańcuchu (NotNullValidator).
/// </summary>
public class FutureDateValidator<T> : ValidationHandler<T>
{
    private readonly Func<T, DateTime?> _valueProvider;
    private readonly string _fieldName;

    public FutureDateValidator(Func<T, DateTime?> valueProvider, string fieldName)
    {
        _valueProvider = valueProvider;
        _fieldName = fieldName;
    }

    protected override Task ValidateAsync(ValidationContext<T> context)
    {
        var value = _valueProvider(context.ItemToValidate);

        // Wykonaj logikę tylko, jeśli wartość nie jest nullem.
        if (value.HasValue && value.Value.Date < DateTime.Today)
        {
            context.Errors.Add(new ValidationError(_fieldName, $"{_fieldName} nie może być z przeszłości."));
        }

        return Task.CompletedTask;
    }
}
