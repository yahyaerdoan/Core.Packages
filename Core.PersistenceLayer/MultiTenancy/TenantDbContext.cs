using Microsoft.EntityFrameworkCore;

namespace Core.PersistenceLayer.MultiTenancy;

/// <summary>Base for a context holding tenant-owned rows: every <see cref="ITenantEntity"/> gets the tenant and soft-delete filters, new rows get the
/// current tenant, and cross-tenant writes are refused, with no per-entity or per-module setup.</summary>
public abstract class TenantDbContext(DbContextOptions options, ITenantContext tenantContext) : DbContext(options), ITenantScopedDbContext
{
    public Guid? CurrentTenantId => TenantScope.Current ?? tenantContext.TenantId;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        _ = optionsBuilder.UseTenantIsolation();
    }
}
