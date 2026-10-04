using System.Text;
using System.Text.Json;
using Core.ApplicationLayer.Pipelines.Cachings.Abstractions;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using ResultHandler.Core.Abstractions;
using StackExchange.Redis;

namespace Core.ApplicationLayer.Pipelines.Cachings.Concretions.CacheBehaviors;

/// <summary>
/// After a successful command, removes its cache key and every key in its cache group. Reads the group from the same place
/// <see cref="CacheAddingBehavior{TRequest, TResponse}"/> wrote it: a Redis set when an IConnectionMultiplexer is registered, otherwise a JSON entry.
/// </summary>
public class CacheRemovingBehavior<TRequest, TResponse>(IDistributedCache distributedCache, IConnectionMultiplexer? redisConnectionMultiplexer = null)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>, ICacheRemoveRequest
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        TResponse response = await next(cancellationToken);

        if (request.ByPassCache || response is IOperationResult { IsSuccessful: false })
        {
            return response;
        }

        List<string> keys = [request.CacheKey];
        if (request.CacheGroupKey is string cacheGroupKey)
        {
            keys.AddRange(await TakeGroupKeysAsync(cacheGroupKey, cancellationToken));
            keys.Add(cacheGroupKey);
            keys.Add(CacheGroupKeys.SlidingExpiration(cacheGroupKey));
        }

        await Task.WhenAll(keys.Distinct().Select(key => distributedCache.RemoveAsync(key, cancellationToken)));
        return response;
    }

    private async Task<IEnumerable<string>> TakeGroupKeysAsync(string cacheGroupKey, CancellationToken cancellationToken)
    {
        if (redisConnectionMultiplexer is IConnectionMultiplexer redis)
        {
            IDatabase redisDatabase = redis.GetDatabase();
            string membersKey = CacheGroupKeys.Members(cacheGroupKey);
            RedisValue[] members = await redisDatabase.SetMembersAsync(membersKey);
            _ = await redisDatabase.KeyDeleteAsync([membersKey, CacheGroupKeys.SlidingExpiration(cacheGroupKey)]);
            return members.Select(member => member.ToString());
        }

        byte[]? cachedGroup = await distributedCache.GetAsync(cacheGroupKey, cancellationToken);
        return cachedGroup is null ? [] : JsonSerializer.Deserialize<HashSet<string>>(Encoding.UTF8.GetString(cachedGroup)) ?? [];
    }
}
