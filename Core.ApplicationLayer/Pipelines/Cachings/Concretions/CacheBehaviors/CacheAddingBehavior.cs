using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Text.Json;

using Core.ApplicationLayer.Pipelines.Cachings.Abstractions;
using Core.ApplicationLayer.Pipelines.Cachings.Concretions.CacheSettings;

using MediatR;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using StackExchange.Redis;

namespace Core.ApplicationLayer.Pipelines.Cachings.Concretions.CacheBehaviors;

public partial class CacheAddingBehavior<TRequest, TResponse>(
    IDistributedCache distributedCache,
    IConfiguration configuration,
    ILogger<CacheAddingBehavior<TRequest, TResponse>> logger,
    IConnectionMultiplexer? redisConnectionMultiplexer = null) :
    IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>, ICacheAddRequest
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> s_keyLocks = new();

    private readonly CacheSetting _cacheSettings =
        configuration.GetSection("CacheSettings").Get<CacheSetting>() ?? throw new InvalidOperationException();

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request.ByPassCache)
        {
            return await next(cancellationToken);
        }

        byte[]? cachedResponse = await distributedCache.GetAsync(request.CacheKey, cancellationToken);

        if (cachedResponse != null && TryDeserialize(cachedResponse, request.CacheKey, out TResponse? deserializedResponse))
        {
            LogFetchedFromCache(request.CacheKey);
            return deserializedResponse;
        }

        SemaphoreSlim localLock = s_keyLocks.GetOrAdd(request.CacheKey, _ => new SemaphoreSlim(1, 1));
        await localLock.WaitAsync(cancellationToken);
        try
        {
            cachedResponse = await distributedCache.GetAsync(request.CacheKey, cancellationToken);
            if (cachedResponse != null && TryDeserialize(cachedResponse, request.CacheKey, out TResponse? doubleCheckedResponse))
            {
                LogFetchedFromCache(request.CacheKey);
                return doubleCheckedResponse;
            }

            return redisConnectionMultiplexer is IConnectionMultiplexer redis
                ? await HandleWithDistributedLockAsync(request, redis, next, cancellationToken)
                : await GetResponseAndAddToCache(request, next, cancellationToken);
        }
        finally
        {
            localLock.Release();
        }
    }

    private async Task<TResponse> HandleWithDistributedLockAsync(TRequest request, IConnectionMultiplexer redis, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        IDatabase redisDatabase = redis.GetDatabase();
        string lockKey = $"lock:{request.CacheKey}";
        string lockToken = Guid.NewGuid().ToString("N");
        TimeSpan lockTimeout = TimeSpan.FromSeconds(_cacheSettings.DistributedLockTimeoutInSeconds);

        bool lockAcquired = await redisDatabase.StringSetAsync(lockKey, lockToken, lockTimeout, When.NotExists);

        if (!lockAcquired)
        {
            TimeSpan pollInterval = TimeSpan.FromMilliseconds(100);
            DateTime deadline = DateTime.UtcNow.Add(lockTimeout);

            while (DateTime.UtcNow < deadline)
            {
                await Task.Delay(pollInterval, cancellationToken);

                byte[]? cachedResponse = await distributedCache.GetAsync(request.CacheKey, cancellationToken);
                if (cachedResponse != null && TryDeserialize(cachedResponse, request.CacheKey, out TResponse? deserializedResponse))
                {
                    LogFetchedFromCache(request.CacheKey);
                    return deserializedResponse;
                }
            }

            return await next(cancellationToken);
        }

        try
        {
            return await GetResponseAndAddToCache(request, next, cancellationToken);
        }
        finally
        {
            const string releaseScript = "if redis.call('get', KEYS[1]) == ARGV[1] then return redis.call('del', KEYS[1]) else return 0 end";
            await redisDatabase.ScriptEvaluateAsync(releaseScript, [lockKey], [lockToken]);
        }
    }

    private bool TryDeserialize(byte[] cachedResponse, string cacheKey, [NotNullWhen(true)] out TResponse? value)
    {
        string json = Encoding.UTF8.GetString(cachedResponse);
        try
        {
            value = JsonSerializer.Deserialize<TResponse>(json);
            return value is not null;
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException)
        {
            LogFailedToDeserializeCache(cacheKey, ex);
            value = default;
            return false;
        }
    }

    private async Task<TResponse> GetResponseAndAddToCache(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        TResponse response = await next(cancellationToken);

        TimeSpan slidingExpiration = request.SlidingExpiration ?? TimeSpan.FromDays(_cacheSettings.SlidingExpiration);
        TimeSpan absoluteCap = TimeSpan.FromDays(_cacheSettings.AbsoluteExpirationCapInDays);

        DistributedCacheEntryOptions distributedCacheEntryOptions = new()
        {
            SlidingExpiration = slidingExpiration,
            AbsoluteExpirationRelativeToNow = absoluteCap
        };

        byte[] serializedData = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response));

        await distributedCache.SetAsync(request.CacheKey, serializedData, distributedCacheEntryOptions, cancellationToken);

        LogAddedToCache(request.CacheKey);

        if (request.CacheGroupKey is string cacheGroupKey)
            await AddCacheKeyToGroup(cacheGroupKey, request.CacheKey, slidingExpiration, cancellationToken);

        return response;
    }

    private async Task AddCacheKeyToGroup(string cacheGroupKey, string cacheKey, TimeSpan slidingExpiration, CancellationToken cancellationToken)
    {
        if (redisConnectionMultiplexer is IConnectionMultiplexer redis)
        {
            await AddCacheKeyToGroupAtomicAsync(redis, cacheGroupKey, cacheKey, slidingExpiration);
            return;
        }

        await AddCacheKeyToGroupBestEffortAsync(cacheGroupKey, cacheKey, slidingExpiration, cancellationToken);
    }

    private static async Task AddCacheKeyToGroupAtomicAsync(IConnectionMultiplexer redis, string cacheGroupKey, string cacheKey, TimeSpan slidingExpiration)
    {
        IDatabase redisDatabase = redis.GetDatabase();
        string groupSetKey = $"{cacheGroupKey}:members";
        string groupExpirationKey = $"{cacheGroupKey}SlidingExpiration";

        await redisDatabase.SetAddAsync(groupSetKey, cacheKey);

        RedisValue existingExpiration = await redisDatabase.StringGetAsync(groupExpirationKey);
        int newExpirationSeconds = Convert.ToInt32(slidingExpiration.TotalSeconds);

        if (!existingExpiration.HasValue || newExpirationSeconds > (int)existingExpiration)
        {
            await redisDatabase.StringSetAsync(groupExpirationKey, newExpirationSeconds, slidingExpiration);
            await redisDatabase.KeyExpireAsync(groupSetKey, slidingExpiration);
        }
    }

    private async Task AddCacheKeyToGroupBestEffortAsync(string cacheGroupKey, string cacheKey, TimeSpan slidingExpiration, CancellationToken cancellationToken)
    {
        byte[]? cacheGroupCache = await distributedCache.GetAsync(cacheGroupKey, cancellationToken);

        HashSet<string> cacheKeysInGroup = cacheGroupCache != null
            ? JsonSerializer.Deserialize<HashSet<string>>(Encoding.UTF8.GetString(cacheGroupCache)) ?? []
            : [];

        if (!cacheKeysInGroup.Add(cacheKey))
            return;

        byte[] newCacheGroupCache = JsonSerializer.SerializeToUtf8Bytes(cacheKeysInGroup);

        byte[]? cacheGroupCacheSlidingExpirationCache = await distributedCache.GetAsync($"{cacheGroupKey}SlidingExpiration", cancellationToken);

        int? cacheGroupCacheSlidingExpirationValue = cacheGroupCacheSlidingExpirationCache != null
            ? Convert.ToInt32(Encoding.UTF8.GetString(cacheGroupCacheSlidingExpirationCache), CultureInfo.InvariantCulture)
            : null;

        if (cacheGroupCacheSlidingExpirationValue == null || slidingExpiration.TotalSeconds > cacheGroupCacheSlidingExpirationValue)
            cacheGroupCacheSlidingExpirationValue = Convert.ToInt32(slidingExpiration.TotalSeconds);

        byte[] serializeCachedGroupSlidingExpirationData = JsonSerializer.SerializeToUtf8Bytes(cacheGroupCacheSlidingExpirationValue);

        DistributedCacheEntryOptions cacheOptions =
            new() { SlidingExpiration = TimeSpan.FromSeconds(Convert.ToDouble(cacheGroupCacheSlidingExpirationValue, CultureInfo.InvariantCulture)) };

        await distributedCache.SetAsync(cacheGroupKey, newCacheGroupCache, cacheOptions, cancellationToken);
        await distributedCache.SetAsync($"{cacheGroupKey}SlidingExpiration", serializeCachedGroupSlidingExpirationData, cacheOptions, cancellationToken);

        LogAddedToCacheGroup(cacheGroupKey);
        LogAddedToCacheGroupSlidingExpiration(cacheGroupKey);
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Fetched from Cache -> {CacheKey}")]
    private partial void LogFetchedFromCache(string cacheKey);

    [LoggerMessage(EventId = 5, Level = LogLevel.Warning, Message = "Failed to deserialize cached response -> {CacheKey}, falling back to a fresh fetch")]
    private partial void LogFailedToDeserializeCache(string cacheKey, Exception exception);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Added to Cache -> {CacheKey}")]
    private partial void LogAddedToCache(string cacheKey);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "Added to Cache -> {CacheGroupKey}")]
    private partial void LogAddedToCacheGroup(string? cacheGroupKey);

    [LoggerMessage(EventId = 4, Level = LogLevel.Information, Message = "Added to Cache -> {CacheGroupKey}SlidingExpiration")]
    private partial void LogAddedToCacheGroupSlidingExpiration(string? cacheGroupKey);
}