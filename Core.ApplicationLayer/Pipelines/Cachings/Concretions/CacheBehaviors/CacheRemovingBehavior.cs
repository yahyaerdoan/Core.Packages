using System.Text;
using System.Text.Json;

using Core.ApplicationLayer.Pipelines.Cachings.Abstractions;

using MediatR;

using Microsoft.Extensions.Caching.Distributed;

namespace Core.ApplicationLayer.Pipelines.Cachings.Concretions.CacheBehaviors;

public class CacheRemovingBehavior<TRequest, TResponse>(IDistributedCache distributedCache)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>, ICacheRemoveRequest
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request.ByPassCache)
        {
            return await next(cancellationToken);
        }
        TResponse response = await next(cancellationToken);
        if (request.CacheGroupKey != null)
        {
            byte[]? cachedGroup = await distributedCache.GetAsync(request.CacheGroupKey, cancellationToken);
            if (cachedGroup != null)
            {
                HashSet<string> keysInGroup = JsonSerializer.Deserialize<HashSet<string>>(Encoding.Default.GetString(cachedGroup))!;
                foreach (string key in keysInGroup)
                {
                    await distributedCache.RemoveAsync(key, cancellationToken);
                }

                await distributedCache.RemoveAsync(request.CacheGroupKey, cancellationToken);
                await distributedCache.RemoveAsync(key: $"{request.CacheGroupKey} SlidingExpiration", cancellationToken);
            }
        }

        if (response != null)
        {
            await distributedCache.RemoveAsync(request.CacheKey, cancellationToken);
        }
        return response;
    }
}
