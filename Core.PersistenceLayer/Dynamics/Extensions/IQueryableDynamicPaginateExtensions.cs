using System.Linq.Expressions;
using Core.PersistenceLayer.Dynamics.Dynamic;
using Core.PersistenceLayer.Pagings.Extensions;
using Core.PersistenceLayer.Pagings.Paging;
using Microsoft.EntityFrameworkCore;

namespace Core.PersistenceLayer.Dynamics.Extensions;

public static class IQueryableDynamicPaginateExtensions
{
    /// <summary>
    /// Read-only filter, sort and page in one call (apply tenant/ownership filters before it). <paramref name="tieBreaker"/> must be unique
    /// (e.g. Id) so rows are never skipped or repeated across pages; <paramref name="defaultDescending"/> flips only the default order, a
    /// client sort always wins. With <paramref name="allowedFields"/> the client may filter and sort only on those.
    /// </summary>
    public static Task<Paginate<T>> ToDynamicPaginateAsync<T>(this IQueryable<T> query, int index, int size, DynamicQuery? dynamicQuery,
        Expression<Func<T, object>> defaultOrderBy, Expression<Func<T, object>> tieBreaker, IReadOnlySet<string>? allowedFields = null,
        bool defaultDescending = false, CancellationToken cancellationToken = default)
        where T : class
    {
        query = query.AsNoTracking();
        if (dynamicQuery is not null)
        {
            query = query.ToDynamic(dynamicQuery, allowedFields);
        }

        bool hasClientSort = dynamicQuery?.Sort?.Any() == true;
        IOrderedQueryable<T> ordered = hasClientSort
            ? (IOrderedQueryable<T>)query
            : defaultDescending ? query.OrderByDescending(defaultOrderBy) : query.OrderBy(defaultOrderBy);

        ordered = !hasClientSort && defaultDescending ? ordered.ThenByDescending(tieBreaker) : ordered.ThenBy(tieBreaker);

        return ordered.ToPaginateAsync(index, size, cancellationToken);
    }
}
