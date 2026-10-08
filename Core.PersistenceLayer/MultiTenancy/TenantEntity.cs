using Core.PersistenceLayer.Repositories.Entities;

namespace Core.PersistenceLayer.MultiTenancy;

/// <summary>Base for tenant-owned rows: TenantId is stamped on save and every query is filtered by it.</summary>
public abstract class TenantEntity<TId> : Entity<TId>, ITenantEntity
{
    public Guid TenantId { get; set; }

    protected TenantEntity()
    {
    }

    protected TenantEntity(TId id) : base(id)
    {
    }
}
