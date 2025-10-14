using System.Reflection;

namespace KERP.Application.Common.Models;

/// <summary>
/// Statyczna klasa pomocnicza do tworzenia obiektów Result.
/// </summary>
public static class ResultFactory
{
    /// <summary>
    /// Tworzy instancję Result.Failure dla podanego typu generycznego TResponse.
    /// </summary>
    /// <typeparam name="TResponse">Typ odpowiedzi, musi dziedziczyć po Result.</typeparam>
    /// <param name="errors">Lista błędów.</param>
    /// <returns>Instancja TResponse reprezentująca porażkę.</returns>
    /// <remarks>
    /// Ta metoda używa reflection aby stworzyć odpowiedni typ Result:
    /// - Dla TResponse = Result → Result.Failure(errors)
    /// - Dla TResponse = Result<ProductDto> → Result<ProductDto>.Failure(errors)
    /// 
    /// Reflection jest potrzebne bo system typów C# nie pozwala na bezpośrednie użycie
    /// generycznego typu TResponse w tym kontekście (limitation kompilatora).
    /// </remarks>
    /// <exception cref="InvalidOperationException">Rzucany, jeśli TResponse nie jest typu Result lub Result<T>.</exception>
    public static TResponse CreateFailure<TResponse>(IReadOnlyCollection<Error> errors) where TResponse : Result
    {
        var responseType = typeof(TResponse);

        // PRZYPADEK 1: TResponse = Result (bez generyka)
        if (responseType == typeof(Result))
        {
            var result = Result.Failure(errors);
            // Rzutowanie jest bezpieczne, ponieważ where TResponse : Result
            return (TResponse)(object)result;
        }

        // PRZYPADEK 2: TResponse = Result<T> (generyczny)
        if (responseType.IsGenericType &&
            responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var valueType = responseType.GetGenericArguments()[0];
            var resultType = typeof(Result<>).MakeGenericType(valueType);

            var failureMethod = resultType.GetMethod(
                "Failure",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(IReadOnlyCollection<Error>) },
                null);

            if (failureMethod == null)
            {
                throw new InvalidOperationException(
                    $"Could not find Failure method on type {resultType.Name}. " +
                    "Ensure Result<T> has a public static Failure method.");
            }

            var result = failureMethod.Invoke(null, new object[] { errors });

            if (result == null)
            {
                throw new InvalidOperationException(
                    $"Failure method on {resultType.Name} returned null.");
            }

            return (TResponse)result;
        }

        // PRZYPADEK 3: TResponse to coś innego (błąd)
        throw new InvalidOperationException(
            $"Unsupported result type: {responseType.Name}. " +
            "This factory only supports Result or Result<T>.");
    }
}