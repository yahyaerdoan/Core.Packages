using System.Text;
using System.Text.Json;
using Core.ApplicationLayer.Pipelines.Cachings.Abstractions;
using Core.PersistenceLayer.MultiTenancy;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace Core.ApplicationLayer.Pipelines.Cachings.Concretions.CacheBehaviors;

/// <summary>
/// Reads each group from the same place <see cref="CacheAddingBehavior{TRequest, TResponse}"/> wrote it: a Redis set when an
/// IConnectionMultiplexer is registered, otherwise a JSON entry. Keys get the same tenant scope the adding behavior gave them.
/// </summary>
public class CacheRemover(IDistributedCache distributedCache, IConnectionMultiplexer? redisConnectionMultiplexer = null, ITenantContext? tenantContext = null)
    : ICacheRemover
{
    public async Task RemoveAsync(string? cacheKey, IReadOnlyCollection<string> cacheGroupKeys, CancellationToken cancellationToken = default)
    {
        // An unknown tenant cached nothing, so there is nothing to remove.
        if (TenantCacheKeys.Prefix(tenantContext) is not string prefix)
        {
            return;
        }

        List<string> keys = string.IsNullOrEmpty(cacheKey) ? [] : [prefix + cacheKey];
        foreach (string groupKey in cacheGroupKeys)
        {
            string cacheGroupKey = prefix + groupKey;
            keys.AddRange(await TakeGroupKeysAsync(cacheGroupKey, cancellationToken));
            keys.Add(cacheGroupKey);
            keys.Add(CacheGroupKeys.SlidingExpiration(cacheGroupKey));
        }

        await Task.WhenAll(keys.Distinct().Select(key => distributedCache.RemoveAsync(key, cancellationToken)));
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
