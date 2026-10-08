using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Core.PersistenceLayer.MultiTenancy;

/// <summary>Applies tenant filters after the context's own OnModelCreating, so where a derived context calls base doesn't matter and entities
/// configured only inside OnModelCreating are covered too. Extends the relational customizer the SQL providers register, keeping its [DbFunction]
/// discovery.</summary>
public sealed class TenantModelCustomizer(ModelCustomizerDependencies dependencies) : RelationalModelCustomizer(dependencies)
{
    public override void Customize(ModelBuilder modelBuilder, DbContext context)
    {
        base.Customize(modelBuilder, context);

        if (context is ITenantScopedDbContext tenantScoped)
        {
            TenantModel.Apply(modelBuilder, tenantScoped);
        }
    }
}
