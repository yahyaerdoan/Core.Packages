using System.Globalization;
using System.Text;
using System.Text.Json;

using Core.ApplicationLayer.Pipelines.Cachings.Abstractions;
using Core.ApplicationLayer.Pipelines.Cachings.Concretions.CacheSettings;

using MediatR;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Core.ApplicationLayer.Pipelines.Cachings.Concretions.CacheBehaviors;

public partial class CacheAddingBehavior<TRequest, TResponse>(IDistributedCache distributedCache, IConfiguration configuration, ILogger<CacheAddingBehavior<TRequest, TResponse>> logger) :
    IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>, ICacheAddRequest
{
    private readonly CacheSetting _cacheSettings =
        configuration.GetSection("CacheSettings").Get<CacheSetting>() ?? throw new InvalidOperationException();

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request.ByPassCache)
        {
            return await next(cancellationToken);
        }

        TResponse? response;

        byte[]? cachedResponse = await distributedCache.GetAsync(request.CacheKey, cancellationToken);

        if (cachedResponse != null)
        {
            response = JsonSerializer.Deserialize<TResponse>(Encoding.Default.GetString(cachedResponse));
            LogFetchedFromCache(request.CacheKey);
        }
        else
        {
            response = await GetResponseAndAddToCache(request, next, cancellationToken);
        }

        return response!;
    }

    private async Task<TResponse?> GetResponseAndAddToCache(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        TResponse response = await next(cancellationToken);

        TimeSpan slidingExpiration = request.SlidingExpiration ?? TimeSpan.FromDays(_cacheSettings.SlidingExpiration);

        DistributedCacheEntryOptions distributedCacheEntryOptions = new() { SlidingExpiration = slidingExpiration };

        byte[] serializedDate = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response));

        await distributedCache.SetAsync(request.CacheKey, serializedDate, distributedCacheEntryOptions, cancellationToken);

        LogAddedToCache(request.CacheKey);

        if (request.CacheGroupKey != null)
            await AddCacheKeyToGroup(request, slidingExpiration, cancellationToken);
        return response;
    }

    private async Task AddCacheKeyToGroup(TRequest request, TimeSpan slidingExpiration, CancellationToken cancellationToken)
    {
        byte[]? cacheGroupCache = await distributedCache.GetAsync(key: request.CacheGroupKey!, cancellationToken);

        HashSet<string> cacheKeysInGroup;

        if (cacheGroupCache != null)
        {
            cacheKeysInGroup = JsonSerializer.Deserialize<HashSet<string>>(Encoding.Default.GetString(cacheGroupCache))!;

            if (!cacheKeysInGroup.Contains(request.CacheKey))
                cacheKeysInGroup.Add(request.CacheKey);
        }
        else
            cacheKeysInGroup = [request.CacheKey];

        byte[] newCacheGroupCache = JsonSerializer.SerializeToUtf8Bytes(cacheKeysInGroup);

        byte[]? cacheGroupCacheSlidingExpirationCache = await distributedCache.GetAsync(key: $"{request.CacheGroupKey}SlidingExpiration", cancellationToken);

        int? cacheGroupCacheSlidingExpirationValue = null;

        if (cacheGroupCacheSlidingExpirationCache != null)
            cacheGroupCacheSlidingExpirationValue = Convert.ToInt32(Encoding.Default.GetString(cacheGroupCacheSlidingExpirationCache), CultureInfo.InvariantCulture);

        if (cacheGroupCacheSlidingExpirationValue == null || slidingExpiration.TotalSeconds > cacheGroupCacheSlidingExpirationValue)
            cacheGroupCacheSlidingExpirationValue = Convert.ToInt32(slidingExpiration.TotalSeconds);

        byte[] serializeCachedGroupSlidingExpirationData = JsonSerializer.SerializeToUtf8Bytes(cacheGroupCacheSlidingExpirationValue);

        DistributedCacheEntryOptions cacheOptions =
            new() { SlidingExpiration = TimeSpan.FromSeconds(Convert.ToDouble(cacheGroupCacheSlidingExpirationValue, CultureInfo.InvariantCulture)) };

        await distributedCache.SetAsync(key: request.CacheGroupKey!, newCacheGroupCache, cacheOptions, cancellationToken);

        LogAddedToCacheGroup(request.CacheGroupKey);

        await distributedCache.SetAsync(key: $"{request.CacheGroupKey}SlidingExpiration", serializeCachedGroupSlidingExpirationData, cacheOptions, cancellationToken);

        LogAddedToCacheGroupSlidingExpiration(request.CacheGroupKey);
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Fetched from Cache -> {CacheKey}")]
    private partial void LogFetchedFromCache(string cacheKey);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Added to Cache -> {CacheKey}")]
    private partial void LogAddedToCache(string cacheKey);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "Added to Cache -> {CacheGroupKey}")]
    private partial void LogAddedToCacheGroup(string? cacheGroupKey);

    [LoggerMessage(EventId = 4, Level = LogLevel.Information, Message = "Added to Cache -> {CacheGroupKey}SlidingExpiration")]
    private partial void LogAddedToCacheGroupSlidingExpiration(string? cacheGroupKey);
}
