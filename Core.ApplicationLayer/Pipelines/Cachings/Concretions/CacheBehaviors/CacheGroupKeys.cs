namespace Core.ApplicationLayer.Pipelines.Cachings.Concretions.CacheBehaviors;

internal static class CacheGroupKeys
{
    public static string Members(string cacheGroupKey) => $"{cacheGroupKey}:members";

    public static string SlidingExpiration(string cacheGroupKey) => $"{cacheGroupKey}SlidingExpiration";
}
