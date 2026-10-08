using Core.PersistenceLayer.Repositories.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Core.PersistenceLayer.MultiTenancy;

public static class TenantIsolationExtensions
{
    /// <summary>Turns on tenant filters and the save guard for an <see cref="ITenantScopedDbContext"/>. <see cref="TenantDbContext"/> calls this itself;
    /// call it for a context that needs another base class.</summary>
    public static DbContextOptionsBuilder UseTenantIsolation(this DbContextOptionsBuilder optionsBuilder)
    {
        _ = optionsBuilder.ReplaceService<IModelCustomizer, TenantModelCustomizer>();

        bool guarded = optionsBuilder.Options.FindExtension<CoreOptionsExtension>()?.Interceptors?.Contains(TenantSaveChangesGuard.Instance) == true;
        return guarded ? optionsBuilder : optionsBuilder.AddInterceptors(TenantSaveChangesGuard.Instance);
    }

    /// <summary>Typed overload of <see cref="UseTenantIsolation(DbContextOptionsBuilder)"/>, so it chains like the provider's own Use* calls.</summary>
    public static DbContextOptionsBuilder<TContext> UseTenantIsolation<TContext>(this DbContextOptionsBuilder<TContext> optionsBuilder) where TContext : DbContext =>
        (DbContextOptionsBuilder<TContext>)UseTenantIsolation((DbContextOptionsBuilder)optionsBuilder);

    /// <summary>Reads across tenants: the one sanctioned way to lift the tenant filter (platform admin screens, cross-module readers that pass an
    /// explicit tenant id). Keeps soft-delete filtering.</summary>
    public static IQueryable<TEntity> IgnoreTenantFilter<TEntity>(this IQueryable<TEntity> query) where TEntity : class =>
        query.IgnoreQueryFilters([QueryFilterNames.Tenant]);

    /// <summary>True when the context builds tenant filters and runs the save guard; for architecture tests.</summary>
    public static bool HasTenantIsolation(this DbContext context) =>
        context is ITenantScopedDbContext
        && context.GetService<IModelCustomizer>() is TenantModelCustomizer
        && context.GetService<IDbContextOptions>().FindExtension<CoreOptionsExtension>()?.Interceptors?.Contains(TenantSaveChangesGuard.Instance) == true;
}
