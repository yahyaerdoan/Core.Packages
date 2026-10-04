using Core.ApplicationLayer.Pipelines.Cachings.Concretions.CacheSettings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Core.ApplicationLayer.Pipelines.Cachings.Extensions;

public static class CachingServiceCollectionExtensions
{
    /// <summary>Binds <see cref="CacheSetting"/> from the "CacheSettings" section once; the app fails at startup when the section is missing or a value isn't positive.</summary>
    public static IServiceCollection AddCacheSettings(this IServiceCollection services, IConfiguration configuration)
    {
        _ = services.AddOptions<CacheSetting>()
            .Bind(configuration.GetSection(CacheSetting.SectionName))
            .Validate(s => s.SlidingExpiration > 0, $"{CacheSetting.SectionName}:{nameof(CacheSetting.SlidingExpiration)} must be greater than 0.")
            .Validate(s => s.AbsoluteExpirationCapInDays > 0, $"{CacheSetting.SectionName}:{nameof(CacheSetting.AbsoluteExpirationCapInDays)} must be greater than 0.")
            .Validate(s => s.DistributedLockTimeoutInSeconds > 0, $"{CacheSetting.SectionName}:{nameof(CacheSetting.DistributedLockTimeoutInSeconds)} must be greater than 0.")
            .ValidateOnStart();
        return services;
    }
}
