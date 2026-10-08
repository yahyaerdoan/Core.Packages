using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Core.PersistenceLayer.MultiTenancy;

/// <summary>Stamps the current tenant on new tenant-owned rows and refuses any save that would touch another tenant's rows, move a row to
/// another tenant, or write tenant-owned rows with no tenant. Acting for another tenant on purpose goes through <see cref="TenantScope"/>.</summary>
public sealed class TenantSaveChangesGuard : SaveChangesInterceptor
{
    public static readonly TenantSaveChangesGuard Instance = new();

    private TenantSaveChangesGuard()
    {
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Guard(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        Guard(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void Guard(DbContext? context)
    {
        if (context is not ITenantScopedDbContext tenantScoped)
        {
            return;
        }

        Guid? tenantId = tenantScoped.CurrentTenantId;

        foreach (EntityEntry<ITenantEntity> entry in context.ChangeTracker.Entries<ITenantEntity>())
        {
            string name = entry.Metadata.DisplayName();

            switch (entry.State)
            {
                case EntityState.Added:
                    if (tenantId is not Guid current)
                    {
                        throw new TenantIsolationException($"Cannot add {name} without a tenant; run it inside TenantScope.Begin.");
                    }

                    if (entry.Entity.TenantId == Guid.Empty)
                    {
                        entry.Entity.TenantId = current;
                    }
                    else if (entry.Entity.TenantId != current)
                    {
                        throw new TenantIsolationException($"Cannot add {name} for tenant {entry.Entity.TenantId} while acting as {current}; run it inside TenantScope.Begin for that tenant.");
                    }

                    break;

                case EntityState.Modified:
                case EntityState.Deleted:
                    PropertyEntry<ITenantEntity, Guid> tenantProperty = entry.Property(e => e.TenantId);
                    Guid owner = tenantProperty.OriginalValue;

                    if (owner != tenantId)
                    {
                        throw new TenantIsolationException($"Cannot change {name} of tenant {owner} while acting as {tenantId?.ToString() ?? "no tenant"}; run it inside TenantScope.Begin for that tenant.");
                    }

                    if (tenantProperty.CurrentValue != owner)
                    {
                        throw new TenantIsolationException($"Cannot move {name} from tenant {owner} to {tenantProperty.CurrentValue}.");
                    }

                    break;
            }
        }
    }
}
