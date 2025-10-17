using KERP.Application.Common.Abstractions.CQRS;
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

    public ValidationChainBuilder<T> WithNotNull<TValue>(Func<T, TValue?> valueProvider, string fieldName)
    {
        AddHandler(new NotNullValidator<T, TValue>(valueProvider, fieldName));
        return this;
    }

    /// <summary>
    /// Dodaje walidację sprawdzającą czy FactoryId użytkownika z cookie
    /// zgadza się z aktualnym FactoryId w bazie danych.
    /// </summary>
    public ValidationChainBuilder<T> WithUserFactory(string fieldName = "UserFactory")
    {
        AddHandler(new UserFactoryValidator<T>(fieldName));
        return this;
    }

    /// <summary>
    /// Dodaje walidację sprawdzającą czy fabryka użytkownika jest aktywna.
    /// </summary>
    public ValidationChainBuilder<T> WithFactoryActive(string fieldName = "Factory")
    {
        AddHandler(new FactoryActiveValidator<T>(fieldName));
        return this;
    }

    /// <summary>
    /// Automatycznie dodaje walidatory kontekstu użytkownika dla requestów
    /// implementujących <see cref="IRequireFactoryValidation"/>.
    /// </summary>
    public ValidationChainBuilder<T> WithFactoryValidationIfRequired()
    {
        // Sprawdzamy czy typ T implementuje IRequireFactoryValidation
        if (typeof(IRequireFactoryValidation).IsAssignableFrom(typeof(T)))
        {
            // Dodajemy walidatory w poprawnej kolejności
            WithUserFactory();   // Najpierw zgodność cookie vs baza
            WithFactoryActive(); // Potem aktywność fabryki
        }
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