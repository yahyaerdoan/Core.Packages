using System.Linq.Expressions;
using System.Reflection;
using Core.PersistenceLayer.Repositories.Entities;
using Core.PersistenceLayer.Repositories.IRepositories;

namespace Core.PersistenceLayer.Ordering;

/// <summary>Keeps one list's display order at 1..n, so users pick a position instead of typing numbers: a new entry goes last, a move
/// shifts the entries between, a delete closes the gap. Siblings narrows the list when entries are ordered per group.</summary>
public sealed class DisplayOrderList<TEntity> where TEntity : Entity<Guid>
{
    private readonly IAsyncRepository<TEntity, Guid> _repository;
    private readonly Expression<Func<TEntity, int>> _order;
    private readonly Func<TEntity, int> _get;
    private readonly PropertyInfo _property;

    public DisplayOrderList(IAsyncRepository<TEntity, Guid> repository, Expression<Func<TEntity, int>> order)
    {
        _repository = repository;
        _order = order;
        _get = order.Compile();
        _property = (order.Body as MemberExpression)?.Member as PropertyInfo
            ?? throw new InvalidOperationException($"'{order}' must point at a property of {typeof(TEntity).Name}.");
    }

    /// <summary>The position after the last entry.</summary>
    public async Task<int> NextAsync(Expression<Func<TEntity, bool>>? siblings = null, CancellationToken cancellationToken = default)
    {
        var entries = await LoadAsync(siblings, cancellationToken);
        return entries.Count == 0 ? 1 : entries.Max(_get) + 1;
    }

    /// <summary>Puts the entry at a 1-based position; a position past either end puts it first or last.</summary>
    public async Task MoveAsync(Guid id, int position, Expression<Func<TEntity, bool>>? siblings = null, CancellationToken cancellationToken = default)
    {
        var entries = (await LoadAsync(siblings, cancellationToken)).ToList();
        if (entries.Find(e => e.Id == id) is not { } moving)
        {
            return;
        }

        _ = entries.Remove(moving);
        entries.Insert(Math.Clamp(position, 1, entries.Count + 1) - 1, moving);
        await RenumberAsync(entries, cancellationToken);
    }

    /// <summary>Applies a whole order, first to last; false when it doesn't list exactly the current entries (the list changed meanwhile).</summary>
    public async Task<bool> ReorderAsync(IReadOnlyList<Guid> orderedIds, Expression<Func<TEntity, bool>>? siblings = null, CancellationToken cancellationToken = default)
    {
        var entries = (await LoadAsync(siblings, cancellationToken)).ToDictionary(e => e.Id);
        if (entries.Count != orderedIds.Count || !orderedIds.All(entries.ContainsKey))
        {
            return false;
        }

        await RenumberAsync([.. orderedIds.Select(id => entries[id])], cancellationToken);
        return true;
    }

    /// <summary>Renumbers the list after an entry left it.</summary>
    public async Task CloseGapAsync(Expression<Func<TEntity, bool>>? siblings = null, CancellationToken cancellationToken = default) =>
        await RenumberAsync([.. await LoadAsync(siblings, cancellationToken)], cancellationToken);

    private Task<IReadOnlyList<TEntity>> LoadAsync(Expression<Func<TEntity, bool>>? siblings, CancellationToken cancellationToken) =>
        _repository.GetAllAsync(siblings, q => q.OrderBy(_order).ThenBy(e => e.CreatedDate).ThenBy(e => e.Id), enableTracking: true, cancellationToken: cancellationToken);

    private async Task RenumberAsync(List<TEntity> entries, CancellationToken cancellationToken)
    {
        var changed = new List<TEntity>();
        for (var index = 0; index < entries.Count; index++)
        {
            if (_get(entries[index]) != index + 1)
            {
                _property.SetValue(entries[index], index + 1);
                changed.Add(entries[index]);
            }
        }

        if (changed.Count > 0)
        {
            _ = await _repository.UpdateRangeAsync(changed, cancellationToken);
        }
    }
}
