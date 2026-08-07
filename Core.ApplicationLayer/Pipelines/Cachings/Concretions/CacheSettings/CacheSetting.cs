namespace Core.ApplicationLayer.Pipelines.Cachings.Concretions.CacheSettings;

public class CacheSetting
{
    public int SlidingExpiration { get; set; }
    public int AbsoluteExpirationCapInDays { get; set; }
    public int DistributedLockTimeoutInSeconds { get; set; }
}
