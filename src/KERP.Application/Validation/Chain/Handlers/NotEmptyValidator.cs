namespace KERP.Application.Validation.Chain.Handlers;

public class NotEmptyValidator<T> : ValidationHandler<T>
{
    private readonly Func<T, string> _valueProvider;
    private readonly string _fieldName;

    public NotEmptyValidator(Func<T, string> valueProvider, string fieldName)
    {
        _valueProvider = valueProvider;
        _fieldName = fieldName;
    }

    protected override Task ValidateAsync(ValidationContext<T> context)
    {
        var value = _valueProvider(context.ItemToValidate);
        if (string.IsNullOrEmpty(value))
        {
            context.Errors.Add(new ValidationError(_fieldName, $"{_fieldName} jest wymagany"));
        }
        return Task.CompletedTask;
    }
}