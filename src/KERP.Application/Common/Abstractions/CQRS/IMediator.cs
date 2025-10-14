namespace KERP.Application.Common.Abstractions.CQRS;

/// <summary>
/// Definiuje kontrakt dla mediatora odpowiedzialnego za routing i przetwarzanie
/// żądań (request) w architekturze CQRS.
/// </summary>
public interface IMediator
{
    /// <summary>
    /// Asynchronicznie wysyła komendę przez pipeline i zwraca result.
    /// </summary>
    /// <typeparam name="TCommand">Typ komendy implementującej <see cref="ICommand{TResult}>">.
    /// Przykład: CreateProductCommand, UpdateProductCommand, DeleteProductCommand.</typeparam>
    /// <typeparam name="TResult">Typ rezultatu zwracango przez handler.
    /// Zazwyczaj: Result, Resuly<T>.</typeparam>
    /// <param name="command">Instancja komendy do przetworzenia.
    /// Command powinien być immutable (record lub klasa readonly properties).</param>
    /// <param name="cancellationToken">Token anulowania operacji.
    /// W kontrolerach używa się HttpContext.RequestAborted.
    /// W Blazor może być CancellationToken.None lub custom token.</param>
    /// <returns>Task zawierający rezultat wykonania komendy.
    /// Rezultat przechodzi przez wszystkie behaviors w pipeline.</returns>
    /// <exception cref="InvalidOperationException">
    /// Rzucany gdy nie znaleziono handlera dla danej komendy.
    /// Handler musi być zarejestrowany w DI.
    /// </exception>
    Task<TResult> SendCommandAsync<TCommand, TResult>(
        TCommand command,
        CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResult>;

    /// <summary>
    /// Asynchronicznie wysyła query przez pipeline i zwraca result.
    /// </summary>
    /// <typeparam name="TQuery">Typ query implementującego <see cref="IQuery{TResult}"/>.
    /// Przykład: GetProductByIdQuery, GetProductsListQuery, SearchProductsQuery
    /// </typeparam>
    /// <typeparam name="TResult">Typ rezultatu zwracango przez handler.
    /// Zazwyczaj: Result, Resuly<T>.</typeparam>
    /// <param name="query">Instancja query do przetworzenia.</param>
    /// Query powinno być immutable (record lub klasa z readonly properties).
    /// <param name="cancellationToken">Token anulowania operacji.
    /// W kontrolerach używa się HttpContext.RequestAborted.
    /// W Blazor może być CancellationToken.None lub custom token.</param>
    /// <returns>Task zawierający rezultat wykonania query.
    /// Rezultat przechodzi przez wszystkie behaviors w pipeline.</returns>
    /// <exception cref="InvalidOperationException">
    /// Rzucany gdy nie znaleziono handlera dla danego query.
    /// Handler musi być zarejestrowany w DI.
    /// </exception>
    Task<TResult> SendQueryAsync<TQuery, TResult>(
        TQuery query,
        CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResult>;
}
