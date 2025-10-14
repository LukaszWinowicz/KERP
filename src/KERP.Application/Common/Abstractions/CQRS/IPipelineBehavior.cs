namespace KERP.Application.Common.Abstractions.CQRS;

/// <summary>
/// Generyczny interfejs dla pipeline behavior, z którego dziedziczą
/// ICommandPipelineBehavior oraz IQueryPipelineBehavior.
/// </summary>
public interface IPipelineBehavior<in TRequest, TResponse>
{
    /// <summary>
    /// Wykonuje logikę behavior'a w pipeline'ie.
    /// </summary>
    /// <returns></returns>
    Task<TResponse> HandleAsync(
        TRequest request,
        Func<Task<TResponse>> next,
        CancellationToken cancellationToken = default);
}
