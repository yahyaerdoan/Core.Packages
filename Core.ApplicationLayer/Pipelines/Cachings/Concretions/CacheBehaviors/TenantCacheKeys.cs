using Core.PersistenceLayer.MultiTenancy;

namespace Core.ApplicationLayer.Pipelines.Cachings.Concretions.CacheBehaviors;

/// <summary>Scopes cache keys to the current tenant, so one tenant never reads another's entry.</summary>
internal static class TenantCacheKeys
{
    /// <summary>Empty for an app without tenants; null when the tenant is unknown, which skips the cache.</summary>
    public static string? Prefix(ITenantContext? tenantContext) => tenantContext switch
    {
        null => string.Empty,
        { TenantId: Guid tenantId } => $"tenant:{tenantId:N}:",
        _ => null,
    };
}
