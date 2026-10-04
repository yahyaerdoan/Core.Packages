namespace Core.ApplicationLayer.Pipelines.Cachings.Concretions.CacheSettings;

/// <summary>Cache lifetimes bound from the "CacheSettings" section by AddCacheSettings; every value must be greater than zero.</summary>
public class CacheSetting
{
    public const string SectionName = "CacheSettings";

    public int SlidingExpiration { get; set; }
    public int AbsoluteExpirationCapInDays { get; set; }
    public int DistributedLockTimeoutInSeconds { get; set; }
}
