using System.Collections;
using System.Linq.Expressions;
using Core.CrossCuttingConcernLayer.ExceptionHandlings.Exceptions;
using Core.PersistenceLayer.Dynamics.Dynamic;
using Core.PersistenceLayer.Dynamics.Extensions;
using Core.PersistenceLayer.Pagings.Extensions;
using Core.PersistenceLayer.Pagings.Paging;
using Core.PersistenceLayer.Repositories.Entities;
using Core.PersistenceLayer.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;

namespace Core.PersistenceLayer.Repositories.EfRepositories;

public class EfRepositoryBase<TEntity, TEntityId, TContext>(TContext context) :
    IAsyncRepository<TEntity, TEntityId> //,IRepository<TEntity, TEntityId>
    where TEntity : Entity<TEntityId>
    where TContext : DbContext
{
    protected TContext Context { get; } = context;

    public async Task<TEntity> AddAsync(TEntity entity)
    {
        entity.CreatedDate = DateTimeOffset.UtcNow;
        _ = await Context.AddAsync(entity);
        _ = await Context.SaveChangesAsync();
        return entity;
    }

    public async Task<ICollection<TEntity>> AddRangeAsync(ICollection<TEntity> entities)
    {
        foreach (TEntity entity in entities)
        {
            entity.CreatedDate = DateTimeOffset.UtcNow;
        }

        await Context.AddRangeAsync(entities);
        _ = await Context.SaveChangesAsync();
        return entities;
    }

    public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>>? predicate = null, bool withDeleted = false,
        bool enableTracking = false, CancellationToken cancellationToken = default)
    {

        IQueryable<TEntity> queryable = Query();
        if (!enableTracking)
        {
            queryable = queryable.AsNoTracking();
        }

        if (withDeleted)
        {
            queryable = queryable.IgnoreQueryFilters([QueryFilterNames.SoftDelete]);
        }

        if (predicate != null)
        {
            queryable = queryable.Where(predicate);
        }

        return await queryable.AnyAsync(cancellationToken);
    }

    public async Task<TEntity> DeleteAsync(TEntity entity, bool permanent = false)
    {
        await SetEntityDeletedAsync(entity, permanent);
        _ = await Context.SaveChangesAsync();
        return entity;
    }

    public async Task<ICollection<TEntity>> DeleteRangeAsync(ICollection<TEntity> entities, bool permanent = false)
    {
        await SetEntityAsDeletedAsync(entities, permanent);
        _ = await Context.SaveChangesAsync();
        return entities;
    }

    public async Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null, bool withDeleted = false,
        bool enableTracking = true, CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> queryable = Query();
        if (!enableTracking)
        {
            queryable = queryable.AsNoTracking();
        }

        if (include != null)
        {
            queryable = include(queryable);
        }

        if (withDeleted)
        {
            queryable = queryable.IgnoreQueryFilters([QueryFilterNames.SoftDelete]);
        }

        if (predicate != null)
        {
            queryable = queryable.Where(predicate);
        }

        return await queryable.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Paginate<TEntity>> GetListAsync(Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null, int index = 0, int size = 10,
        bool withDeleted = false, bool enableTracking = false,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> queryable = Query();
        if (!enableTracking)
        {
            queryable = queryable.AsNoTracking();
        }

        if (include != null)
        {
            queryable = include(queryable);
        }

        if (withDeleted)
        {
            queryable = queryable.IgnoreQueryFilters([QueryFilterNames.SoftDelete]);
        }

        if (predicate != null)
        {
            queryable = queryable.Where(predicate);
        }

        return await (orderBy ?? OrderById)(queryable).ToPaginateAsync(index, size, cancellationToken);
    }

    public async Task<Paginate<TEntity>> GetListByDynamicAsync(DynamicQuery dynamicQuery,
        Expression<Func<TEntity, bool>>? predicate = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null, int index = 0, int size = 10,
        bool withDeleted = false, bool enableTracking = false, CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> queryable = Query();
        if (!enableTracking)
        {
            queryable = queryable.AsNoTracking();
        }

        if (include != null)
        {
            queryable = include(queryable);
        }

        if (withDeleted)
        {
            queryable = queryable.IgnoreQueryFilters([QueryFilterNames.SoftDelete]);
        }

        if (predicate != null)
        {
            queryable = queryable.Where(predicate);
        }

        if (dynamicQuery.Sort is null || !dynamicQuery.Sort.Any())
        {
            queryable = (orderBy ?? OrderById)(queryable);
        }

        return await queryable.ToDynamic(dynamicQuery).ToPaginateAsync(index, size, cancellationToken);
    }

    public IQueryable<TEntity> Query()
    {
        return Context.Set<TEntity>();
    }

    public async Task<TEntity> UpdateAsync(TEntity entity)
    {
        entity.UpdatedDate = DateTimeOffset.UtcNow;
        AttachForUpdate(entity);
        _ = await Context.SaveChangesAsync();
        return entity;
    }

    public async Task<ICollection<TEntity>> UpdateRangeAsync(ICollection<TEntity> entities)
    {
        foreach (TEntity entity in entities)
        {
            entity.UpdatedDate = DateTimeOffset.UtcNow;
            AttachForUpdate(entity);
        }

        _ = await Context.SaveChangesAsync();
        return entities;
    }

    #region Extensions Methods
    protected async Task SetEntityDeletedAsync(TEntity entity, bool permanent)
    {
        if (!permanent)
        {
            CheckHasEntityHaveOneToOneRelation(entity);
            await SetEntityAsSoftDeletedAsync(entity);
        }
        else
        {
            _ = Context.Remove(entity);
        }
    }
    protected void CheckHasEntityHaveOneToOneRelation(TEntity entity)
    {
        bool hasEntityHaveOneToOneRelation = Context.Entry(entity).Metadata.GetForeignKeys().Any(fk => fk.IsUnique);
        if (hasEntityHaveOneToOneRelation)
        {
            throw new BusinessRuleException(
                "Entity has one-to-one relationship. Soft Delete causes problems if you try to create entry again by same foreign key."
            );
        }
    }
    protected async Task SetEntityAsSoftDeletedAsync(IEntityTimeStamps entity)
    {
        if (entity.DeletedDate.HasValue)
        {
            return;
        }

        entity.DeletedDate = DateTimeOffset.UtcNow;

        List<INavigation> navigations = Context
            .Entry(entity)
            .Metadata.GetNavigations()
            .Where(x => x is { IsOnDependent: false, ForeignKey.DeleteBehavior: DeleteBehavior.ClientCascade or DeleteBehavior.Cascade })
            .ToList();
        foreach (INavigation? navigation in navigations)
        {
            if (navigation.TargetEntityType.IsOwned())
            {
                continue;
            }

            if (navigation.PropertyInfo == null)
            {
                continue;
            }

            if (navigation.IsCollection)
            {
                CollectionEntry collection = Context.Entry(entity).Collection(navigation.PropertyInfo.Name);
                IEnumerable children = collection.IsLoaded && navigation.PropertyInfo.GetValue(entity) is IEnumerable loaded
                    ? loaded.Cast<object>().ToList()
                    : await GetRelationLoaderQuery(collection.Query()).ToListAsync();

                foreach (IEntityTimeStamps child in children)
                {
                    await SetEntityAsSoftDeletedAsync(child);
                }
            }
            else
            {
                ReferenceEntry reference = Context.Entry(entity).Reference(navigation.PropertyInfo.Name);
                object? child = reference.IsLoaded
                    ? navigation.PropertyInfo.GetValue(entity)
                    : await GetRelationLoaderQuery(reference.Query()).FirstOrDefaultAsync();

                if (child is IEntityTimeStamps timeStamped)
                {
                    await SetEntityAsSoftDeletedAsync(timeStamped);
                }
            }
        }

        AttachForUpdate(entity);
    }

    /// <summary>
    /// Marks an entity the context doesn't track yet as modified (with everything reachable from it). An entity loaded with tracking is
    /// left to the change tracker, so SaveChanges writes only the rows and columns that actually changed.
    /// </summary>
    protected void AttachForUpdate(object entity)
    {
        if (Context.Entry(entity).State == EntityState.Detached)
        {
            _ = Context.Update(entity);
        }
    }

    /// <summary>Default order for paging when the caller gives none, so pages are stable across calls.</summary>
    protected static IOrderedQueryable<TEntity> OrderById(IQueryable<TEntity> queryable) => queryable.OrderBy(e => e.Id);

    protected IQueryable<object> GetRelationLoaderQuery(IQueryable query)
    {
        return query.Cast<object>().Where(x => !((IEntityTimeStamps)x).DeletedDate.HasValue);
    }

    protected async Task SetEntityAsDeletedAsync(IEnumerable<TEntity> entities, bool permanent)
    {
        foreach (TEntity entity in entities)
        {
            await SetEntityDeletedAsync(entity, permanent);
        }
    }

    #endregion
}
