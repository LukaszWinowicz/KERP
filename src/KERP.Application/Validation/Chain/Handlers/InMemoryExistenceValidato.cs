namespace KERP.Application.Validation.Chain.Handlers;

/// <summary>
/// Sprawdza, czy podany klucz istnieje w dostarczonym zbiorze danych (w pamięci).
/// </summary>
public class InMemoryExistenceValidator<T> : ValidationHandler<T>
{
    public readonly Func<T, string> _keyProvider;
    public readonly HashSet<string> _existingKeys;
    private readonly string _fieldName;

    public InMemoryExistenceValidator(
        Func<T, string> keyProvider,
        HashSet<string> existingKeys,
        string fieldName)
    {
        _keyProvider = keyProvider;
        _existingKeys = existingKeys;
        _fieldName = fieldName;
    }

    protected override Task ValidateAsync(ValidationContext<T> context)
    {
        var key = _keyProvider(context.ItemToValidate);

        if (!_existingKeys.Contains(key))
        {
            context.Errors.Add(new ValidationError(_fieldName, $"Wartość '{key}' dla pola {_fieldName} nie istieje w systemie."));
        }
        return Task.CompletedTask;
    }
}