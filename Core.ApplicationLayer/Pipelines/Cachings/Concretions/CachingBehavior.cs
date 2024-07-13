using Core.ApplicationLayer.Pipelines.Cachings.Abstractions;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Core.ApplicationLayer.Pipelines.Cachings.Concretions;

public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>, ICachableRequest
{
    private readonly CacheSettings _cacheSettings;
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;

    public CachingBehavior(IDistributedCache distributedCache, IConfiguration configuration, ILogger<CachingBehavior<TRequest, TResponse>> logger)
    {
        _cacheSettings = configuration.GetSection("CacheSettings").Get<CacheSettings>() ?? throw new InvalidOperationException();
        _distributedCache = distributedCache;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request.ByPassCache)
        {
            return await next();
        }
        TResponse response;
        byte[] cachedResponse = await _distributedCache.GetAsync(request.CacheKey, cancellationToken);
        if (cachedResponse != null)
        {
            response = JsonSerializer.Deserialize<TResponse>(Encoding.Default.GetString(cachedResponse));
            _logger.LogInformation($"Fetched from Cache -> {request.CacheKey}");
        }
        else 
        {
            response = await GetResponseAndAddToCache(request,next, cancellationToken);
        }
        return response;
    }

    private async Task<TResponse?> GetResponseAndAddToCache(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        TResponse response = await next();
        TimeSpan slidingExpiration = request.SlidingExpiration ?? TimeSpan.FromDays(_cacheSettings.SlidingExpiration);
        DistributedCacheEntryOptions distributedCacheEntryOptions = new() { SlidingExpiration = slidingExpiration };
        byte[] serializedDate = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response));
        await _distributedCache.SetAsync(request.CacheKey, serializedDate, cancellationToken);
        _logger.LogInformation($"Added to Cache -> {request.CacheKey}");
        return response;
    }
}
