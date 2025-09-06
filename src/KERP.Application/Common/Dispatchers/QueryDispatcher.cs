using KERP.Application.Common.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace KERP.Application.Common.Dispatchers;

public sealed class QueryDispatcher : IQueryDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public QueryDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task<TResult> Send<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
    {
        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));
        var handler = _serviceProvider.GetRequiredService(handlerType);

        return (Task<TResult>)handlerType.GetMethod("Handle")!
            .Invoke(handler, new object[] { query, cancellationToken })!;
    }
}
