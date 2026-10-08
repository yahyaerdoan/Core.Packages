namespace Core.PersistenceLayer.MultiTenancy;

/// <summary>A context holding tenant-owned rows. Derive from <see cref="TenantDbContext"/>, or implement this and call UseTenantIsolation when the
/// context needs another base class (e.g. an Identity context).</summary>
public interface ITenantScopedDbContext
{
    /// <summary>The tenant this context reads and writes as: an open <see cref="TenantScope"/> first, then the ambient tenant.</summary>
    Guid? CurrentTenantId { get; }
}
