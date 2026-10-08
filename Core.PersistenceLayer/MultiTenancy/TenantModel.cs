using System.Linq.Expressions;
using Core.PersistenceLayer.Repositories.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.PersistenceLayer.MultiTenancy;

/// <summary>Adds the tenant (and soft-delete) filter to every <see cref="ITenantEntity"/> in the model. The filter reads CurrentTenantId from the
/// context as a constant, which EF swaps for the context running each query; anything else captured here would be evaluated once and reused for
/// every tenant.</summary>
internal static class TenantModel
{
    public static void Apply(ModelBuilder modelBuilder, ITenantScopedDbContext context)
    {
        Expression currentTenantId = Expression.Property(Expression.Constant(context, context.GetType()), nameof(ITenantScopedDbContext.CurrentTenantId));

        foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes().ToList())
        {
            if (entityType.BaseType is not null || entityType.IsOwned() || !typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            ParameterExpression row = Expression.Parameter(entityType.ClrType, "row");
            Expression rowTenantId = Expression.Convert(Expression.Property(row, nameof(ITenantEntity.TenantId)), typeof(Guid?));
            EntityTypeBuilder builder = modelBuilder.Entity(entityType.ClrType);

            _ = builder.HasQueryFilter(QueryFilterNames.Tenant, Expression.Lambda(Expression.Equal(rowTenantId, currentTenantId), row));

            if (typeof(IEntityTimeStamps).IsAssignableFrom(entityType.ClrType))
            {
                Expression deletedDate = Expression.Property(row, nameof(IEntityTimeStamps.DeletedDate));
                _ = builder.HasQueryFilter(QueryFilterNames.SoftDelete, Expression.Lambda(Expression.Equal(deletedDate, Expression.Constant(null, typeof(DateTimeOffset?))), row));
                _ = builder.HasIndex(nameof(ITenantEntity.TenantId), nameof(IEntityTimeStamps.DeletedDate));
            }
            else
            {
                _ = builder.HasIndex(nameof(ITenantEntity.TenantId));
            }
        }
    }
}
