namespace Core.ApplicationLayer.Pipelines.Cachings.Abstractions;

public interface ICacheRemoveRequest
{
    string CacheKey { get; }
    string? CacheGroupKey { get; }
    bool ByPassCache { get; }
}
