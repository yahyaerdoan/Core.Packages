using Core.PersistenceLayer.Repositories.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Core.PersistenceLayer.Repositories.Auditing;

/// <summary>
/// Stamps audit fields on every save, including entities added through a navigation. Created values are set once and never overwritten by an
/// update; a modified row whose DeletedDate was just set counts as a soft delete and only gets DeletedBy.
/// </summary>
public class AuditSaveChangesInterceptor(IAuditUserProvider auditUserProvider, TimeProvider timeProvider) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Stamp(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        Stamp(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void Stamp(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        string? userId = auditUserProvider.UserId;
        DateTimeOffset now = timeProvider.GetUtcNow();

        foreach (EntityEntry<IEntityTimeStamps> entry in context.ChangeTracker.Entries<IEntityTimeStamps>())
        {
            IEntityAuditor? auditor = entry.Entity as IEntityAuditor;

            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.CreatedDate == default)
                {
                    entry.Entity.CreatedDate = now;
                }

                if (auditor is not null)
                {
                    auditor.CreatedBy ??= userId;
                }

                continue;
            }

            if (entry.State != EntityState.Modified)
            {
                continue;
            }

            entry.Property(nameof(IEntityTimeStamps.CreatedDate)).IsModified = false;
            if (auditor is not null)
            {
                entry.Property(nameof(IEntityAuditor.CreatedBy)).IsModified = false;
            }

            bool isSoftDelete = entry.Property(nameof(IEntityTimeStamps.DeletedDate)).IsModified && entry.Entity.DeletedDate.HasValue;
            if (isSoftDelete)
            {
                if (auditor is not null)
                {
                    auditor.DeletedBy ??= userId;
                }

                continue;
            }

            entry.Entity.UpdatedDate = now;
            if (auditor is not null)
            {
                auditor.UpdatedBy = userId;
            }
        }
    }
}
