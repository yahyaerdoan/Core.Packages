using Core.ApplicationLayer.Pipelines.Cachings.Abstractions;
using Core.PersistenceLayer.MultiTenancy;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using ResultHandler.Core.Abstractions;
using StackExchange.Redis;

namespace Core.ApplicationLayer.Pipelines.Cachings.Concretions.CacheBehaviors;

/// <summary>After a successful command, removes its cache key and every key in its cache group through <see cref="CacheRemover"/>.</summary>
public class CacheRemovingBehavior<TRequest, TResponse>(IDistributedCache distributedCache, IConnectionMultiplexer? redisConnectionMultiplexer = null, ITenantContext? tenantContext = null)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>, ICacheRemoveRequest
{
    private readonly CacheRemover _cacheRemover = new(distributedCache, redisConnectionMultiplexer, tenantContext);

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        TResponse response = await next(cancellationToken);

        if (request.ByPassCache || response is IOperationResult { IsSuccessful: false })
        {
            return response;
        }

        await _cacheRemover.RemoveAsync(request.CacheKey, request.CacheGroupKey is string groupKey ? [groupKey] : [], cancellationToken);
        return response;
    }
}
