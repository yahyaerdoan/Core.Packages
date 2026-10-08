namespace Core.PersistenceLayer.MultiTenancy;

/// <summary>Runs code as a given tenant, overriding the ambient one, e.g. a webhook or background job acting for a tenant, or a platform admin
/// creating a tenant's first user. Flows across awaits; dispose to restore the previous tenant.</summary>
public static class TenantScope
{
    private static readonly AsyncLocal<Guid?> Ambient = new();

    /// <summary>The tenant set by the innermost open scope, or null outside any scope.</summary>
    public static Guid? Current => Ambient.Value;

    /// <summary>Acts as <paramref name="tenantId"/> until the returned scope is disposed.</summary>
    public static IDisposable Begin(Guid tenantId)
    {
        Guid? previous = Ambient.Value;
        Ambient.Value = tenantId;
        return new Scope(previous);
    }

    private sealed class Scope(Guid? previousTenantId) : IDisposable
    {
        public void Dispose() => Ambient.Value = previousTenantId;
    }
}
