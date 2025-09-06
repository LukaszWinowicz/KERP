namespace KERP.Application.Common.Abstractions;

public interface IQueryDispatcher
{
    Task<TResult> Send<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default);
}
