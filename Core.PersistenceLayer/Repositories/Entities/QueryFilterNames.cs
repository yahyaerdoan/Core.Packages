namespace Core.PersistenceLayer.Repositories.Entities;

/// <summary>Named query filter keys - lets withDeleted ignore only soft-delete, not every filter (e.g. tenant isolation) an entity might also carry.</summary>
public static class QueryFilterNames
{
    public const string SoftDelete = "SoftDelete";

    /// <summary>Tenant isolation; lift it only through IgnoreTenantFilter, never IgnoreQueryFilters directly.</summary>
    public const string Tenant = "Tenant";
}
