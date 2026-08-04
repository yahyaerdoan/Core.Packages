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
        await Context.AddAsync(entity);
        await Context.SaveChangesAsync();
        return entity;
    }

    public async Task<ICollection<TEntity>> AddRangeAsync(ICollection<TEntity> entities)
    {
        foreach (TEntity entity in entities)
        {
            entity.CreatedDate = DateTimeOffset.UtcNow;
            await Context.AddRangeAsync(entity);
            await Context.SaveChangesAsync();
        }
        return entities;
    }

    public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>>? predicate = null, bool withDeleted = false,
        bool enableTracking = false, CancellationToken cancellationToken = default)
    {

        IQueryable<TEntity> queryable = Query();
        if (!enableTracking) queryable = queryable.AsNoTracking();
        if (withDeleted) queryable = queryable.IgnoreQueryFilters();
        if (predicate != null) queryable = queryable.Where(predicate);
        return await queryable.AnyAsync(cancellationToken);
    }

    public async Task<TEntity> DeleteAsync(TEntity entity, bool permanent = false)
    {
        await SetEntityDeletedAsync(entity, permanent);
        await Context.SaveChangesAsync();
        return entity;
    }

    public async Task<ICollection<TEntity>> DeleteRangeAsync(ICollection<TEntity> entities, bool permanent = false)
    {
        await SetEntityAsDeletedAsync(entities, permanent);
        await Context.SaveChangesAsync();
        return entities;
    }

    public async Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null, bool withDeleted = false,
        bool enableTracking = true, CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> queryable = Query();
        if (!enableTracking) queryable = queryable.AsNoTracking();
        if (include != null) queryable = include(queryable);
        if (withDeleted) queryable = queryable.IgnoreQueryFilters();
        if (predicate != null) queryable = queryable.Where(predicate);
        return await queryable.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Paginate<TEntity>> GetListAsync(Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null, int index = 0, int size = 10,
        bool withDeleted = false, bool enableTracking = false,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> queryable = Query();
        if (!enableTracking) queryable = queryable.AsNoTracking();
        if (include != null) queryable = include(queryable);
        if (withDeleted) queryable = queryable.IgnoreQueryFilters();
        if (predicate != null) queryable = queryable.Where(predicate);
        return orderBy != null
            ? await orderBy(queryable).ToPaginateAsync(index, size, cancellationToken)
            : await queryable.ToPaginateAsync(index, size, cancellationToken);
    }

    public async Task<Paginate<TEntity>> GetListByDynamicAsync(DynamicQuery dynamicQuery,
        Expression<Func<TEntity, bool>>? predicate = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null, int index = 0, int size = 10,
        bool withDeleted = false, bool enableTracking = false, CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> queryable = Query().ToDynamic(dynamicQuery);
        if (!enableTracking) queryable = queryable.AsNoTracking();
        if (include != null) queryable = include(queryable);
        if (withDeleted) queryable = queryable.IgnoreQueryFilters();
        if (predicate != null) queryable = queryable.Where(predicate);
        return await queryable.ToPaginateAsync(index, size, cancellationToken);
    }

    public IQueryable<TEntity> Query() => Context.Set<TEntity>();

    public async Task<TEntity> UpdateAsync(TEntity entity)
    {
        entity.UpdatedDate = DateTimeOffset.UtcNow;
        Context.Update(entity);
        await Context.SaveChangesAsync();
        return entity;
    }

    public async Task<ICollection<TEntity>> UpdateRangeAsync(ICollection<TEntity> entities)
    {
        foreach (TEntity entity in entities)
        {
            entity.UpdatedDate = DateTimeOffset.UtcNow;
            Context.UpdateRange(entity);
            await Context.SaveChangesAsync();
        }
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
            Context.Remove(entity);
        }
    }
    protected void CheckHasEntityHaveOneToOneRelation(TEntity entity)
    {
        bool hasEntityHaveOneToOneRelation = Context.Entry(entity).Metadata.GetForeignKeys().Any(fk => fk.IsUnique);
        if (hasEntityHaveOneToOneRelation)
            throw new BusinessRuleException(
                "Entity has one-to-one relationship. Soft Delete causes problems if you try to create entry again by same foreign key."
            );
    }
    protected async Task SetEntityAsSoftDeletedAsync(IEntityTimeStamps entity)
    {
        if (entity.DeletedDate.HasValue)
            return;
        entity.DeletedDate = DateTimeOffset.UtcNow;

        var navigations = Context
            .Entry(entity)
            .Metadata.GetNavigations()
            .Where(x => x is { IsOnDependent: false, ForeignKey.DeleteBehavior: DeleteBehavior.ClientCascade or DeleteBehavior.Cascade })
            .ToList();
        foreach (INavigation? navigation in navigations)
        {
            if (navigation.TargetEntityType.IsOwned())
                continue;
            if (navigation.PropertyInfo == null)
                continue;

            object? navValue = navigation.PropertyInfo.GetValue(entity);
            if (navigation.IsCollection)
            {
                if (navValue == null)
                {
                    IQueryable query = Context.Entry(entity).Collection(navigation.PropertyInfo.Name).Query();
                    navValue = await GetRelationLoaderQuery(query).ToListAsync();
                    if (navValue == null)
                        continue;
                }

                foreach (IEntityTimeStamps navValueItem in (IEnumerable)navValue)
                    await SetEntityAsSoftDeletedAsync(navValueItem);
            }
            else
            {
                if (navValue == null)
                {
                    IQueryable query = Context.Entry(entity).Reference(navigation.PropertyInfo.Name).Query();
                    navValue = await GetRelationLoaderQuery(query).FirstOrDefaultAsync();
                    if (navValue == null)
                        continue;
                }

                await SetEntityAsSoftDeletedAsync((IEntityTimeStamps)navValue);
            }
        }

        Context.Update(entity);
    }
    protected IQueryable<object> GetRelationLoaderQuery(IQueryable query) =>
        query.Cast<object>().Where(x => !((IEntityTimeStamps)x).DeletedDate.HasValue);
    protected async Task SetEntityAsDeletedAsync(IEnumerable<TEntity> entities, bool permanent)
    {
        foreach (TEntity entity in entities)
            await SetEntityDeletedAsync(entity, permanent);
    }

    #endregion
}
