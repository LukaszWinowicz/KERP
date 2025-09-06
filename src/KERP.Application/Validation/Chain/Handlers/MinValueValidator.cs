namespace KERP.Application.Validation.Chain.Handlers;

public class MinValueValidator<T, TValue> : ValidationHandler<T> where TValue : IComparable<TValue>
{
    private readonly Func<T, TValue> _valueProvider;
    private readonly TValue _minValue;
    private readonly string _fieldName;

    public MinValueValidator(Func<T, TValue> valueProvider, TValue minValue, string fieldName)
    {
        _valueProvider = valueProvider ?? throw new ArgumentNullException(nameof(valueProvider));
        _minValue = minValue;
        _fieldName = fieldName ?? throw new ArgumentNullException(nameof(fieldName));
    }

    protected override Task ValidateAsync(ValidationContext<T> context)
    {
        var value = _valueProvider(context.ItemToValidate);
        if (value.CompareTo(_minValue) < 0)
        {
            context.Errors.Add(new ValidationError(_fieldName, $"{_fieldName} musi być większy lub równy {_minValue}."));
        }
        return Task.CompletedTask;
    }
}