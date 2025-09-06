namespace KERP.Application.Validation.Chain.Handlers;

/// <summary>
/// Generyczny walidator sprawdzający, czy podana właściwość nie ma wartości null.
/// Działa zarówno dla typów referencyjnych (klas) jak i nullable typów wartości (struct?).
/// </summary>
public class NotNullValidator<T, TValue> : ValidationHandler<T>
{
    private readonly Func<T, TValue?> _valueProvider;
    private readonly string _fieldName;

    public NotNullValidator(Func<T, TValue?> valueProvider, string fieldName)
    {
        _valueProvider = valueProvider;
        _fieldName = fieldName;
    }

    protected override Task ValidateAsync(ValidationContext<T> context)
    {
        var value = _valueProvider(context.ItemToValidate);

        // Warunek 'is null' działa poprawnie zarówno dla klas, jak i dla typów Nullable<T> (np. DateTime?)
        if (value is null)
        {
            context.Errors.Add(new ValidationError(_fieldName, $"Wartość dla pola {_fieldName} jest wymagana."));
        }

        return Task.CompletedTask;
    }
}
