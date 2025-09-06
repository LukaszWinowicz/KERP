namespace KERP.Application.Validation.Chain.Handlers;

public class StringLengthValidator<T> : ValidationHandler<T>
{
    private readonly Func<T, string> _valueProvider;
    private readonly int _exactLength;
    private readonly string _fieldName;

    public StringLengthValidator(Func<T, string> valueProvider, int exactLength, string fieldName)
    {
        _valueProvider = valueProvider;
        _exactLength = exactLength;
        _fieldName = fieldName;
    }

    protected override Task ValidateAsync(ValidationContext<T> context)
    {
        var value = _valueProvider(context.ItemToValidate);
        if (value.Length != _exactLength)
        {
            context.Errors.Add(new ValidationError(_fieldName, $"{_fieldName} musi mieć dokładnie {_exactLength} znaków"));
        }
        return Task.CompletedTask;
    }
}
