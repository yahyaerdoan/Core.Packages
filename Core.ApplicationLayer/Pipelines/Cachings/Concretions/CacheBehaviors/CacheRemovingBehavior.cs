using Core.ApplicationLayer.Pipelines.Cachings.Abstractions;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Core.ApplicationLayer.Pipelines.Cachings.Concretions.CacheBehaviors;

public class CacheRemovingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>, ICacheRemoveRequest
{
    private readonly IDistributedCache _distributedCache;

    public CacheRemovingBehavior(IDistributedCache distributedCache)
    {
        _distributedCache = distributedCache;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request.ByPassCache)
        {
            return await next();
        }
        TResponse response = await next();
        if (request.CacheGroupKey != null)
        {
            byte[]? cachedGroup = await _distributedCache.GetAsync(request.CacheGroupKey, cancellationToken);
            if (cachedGroup != null)
            {
                HashSet<string> keysInGroup = JsonSerializer.Deserialize<HashSet<string>>(Encoding.Default.GetString(cachedGroup))!;
                foreach (string key in keysInGroup)
                {
                    await _distributedCache.RemoveAsync(key, cancellationToken);
                }

                await _distributedCache.RemoveAsync(request.CacheGroupKey, cancellationToken);
                await _distributedCache.RemoveAsync(key: $"{request.CacheGroupKey} SlidingExpiration", cancellationToken);
            }
        }

        if (response != null)
        {
            await _distributedCache.RemoveAsync(request.CacheKey, cancellationToken);
        }
        return response;
    }
}
