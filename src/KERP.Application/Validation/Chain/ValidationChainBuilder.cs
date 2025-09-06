using KERP.Application.Validation.Chain.Handlers;

namespace KERP.Application.Validation.Chain;

/// <summary>
/// Umożliwia płynne budowanie łańucha walidacji dla dowlonego typu.
/// </summary>
/// <typeparam name="T"></typeparam>
public class ValidationChainBuilder<T>
{
    private ValidationHandler<T>? _head;
    private ValidationHandler<T>? _tail;

    private void AddHandler(ValidationHandler<T> handler)
    {
        if (_head == null)
        {
            _head = handler;
            _tail = handler;
        }
        else
        {
            _tail!.SetNext(handler);
            _tail = handler;
        }
    }

    public ValidationChainBuilder<T> WithNotEmpty(Func<T, string> valueProvider, string fieldName)
    {
        AddHandler(new NotEmptyValidator<T>(valueProvider, fieldName));
        return this;
    }

    public ValidationChainBuilder<T> WithStringLength(Func<T, string> valueProvider, int exactLength, string fieldName)
    {
        AddHandler(new StringLengthValidator<T>(valueProvider, exactLength, fieldName));
        return this;
    }

    public ValidationChainBuilder<T> WithMinValue<TValue>(Func<T, TValue> valueProvider, TValue minValue, string fieldName)
        where TValue : IComparable<TValue>
    {
        AddHandler(new MinValueValidator<T, TValue>(valueProvider, minValue, fieldName));
        return this;
    }

    public ValidationChainBuilder<T> WithFutureDate(Func<T, DateTime?> valueProvider, string fieldName)
    {
        AddHandler(new FutureDateValidator<T>(valueProvider, fieldName));
        return this;
    }

    public ValidationChainBuilder<T> WithInMemoryExistence(Func<T, string> keyProvider, HashSet<string> existingKeys, string fieldName)
    {
        AddHandler(new InMemoryExistenceValidator<T>(keyProvider, existingKeys, fieldName));
        return this;

    }

    /// <summary>
    /// Dodaje do łańcucha regułę sprawdzającą, czy wartość nie jest nullem.
    /// </summary>
    public ValidationChainBuilder<T> WithNotNull<TValue>(Func<T, TValue?> valueProvider, string fieldName)
    {
        AddHandler(new NotNullValidator<T, TValue>(valueProvider, fieldName));
        return this;
    }


    /// <summary>
    /// Buduje łańcuch i zwraca jego pierwsze ogniwo.
    /// </summary>
    public ValidationHandler<T> Build()
    {
        if (_head == null)
        {
            throw new InvalidOperationException("Łańcuch walidacji nie zawiera żadnych handlerów.");
        }
        return _head;
    }

}