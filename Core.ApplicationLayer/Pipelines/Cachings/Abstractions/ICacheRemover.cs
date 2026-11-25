namespace Core.ApplicationLayer.Pipelines.Cachings.Abstractions;

/// <summary>Removes the current tenant's cached entries; for a write that clears more than one <see cref="ICacheRemoveRequest"/> group.</summary>
public interface ICacheRemover
{
    /// <summary>Removes <paramref name="cacheKey"/> and every entry of <paramref name="cacheGroupKeys"/>.</summary>
    Task RemoveAsync(string? cacheKey, IReadOnlyCollection<string> cacheGroupKeys, CancellationToken cancellationToken = default);
}
