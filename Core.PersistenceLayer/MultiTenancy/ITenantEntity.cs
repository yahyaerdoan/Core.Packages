namespace Core.PersistenceLayer.MultiTenancy;

/// <summary>A row owned by one tenant. A context using tenant isolation filters every query on it and stamps it on insert, with no per-entity setup.</summary>
public interface ITenantEntity
{
    Guid TenantId { get; set; }
}
