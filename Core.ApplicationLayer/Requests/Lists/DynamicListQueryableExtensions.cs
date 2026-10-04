using System.Linq.Expressions;
using Core.PersistenceLayer.Dynamics.Extensions;
using Core.PersistenceLayer.Pagings.Paging;

namespace Core.ApplicationLayer.Requests.Lists;

public static class DynamicListQueryableExtensions
{
    /// <summary>Pages, filters and sorts by the request, allowing only its <see cref="IDynamicListRequest.QueryableFields"/>.</summary>
    public static Task<Paginate<T>> ToDynamicPaginateAsync<T, TRequest>(this IQueryable<T> query, TRequest request, Expression<Func<T, object>> defaultOrderBy,
        Expression<Func<T, object>> tieBreaker, bool defaultDescending = false, CancellationToken cancellationToken = default)
        where T : class
        where TRequest : IDynamicListRequest =>
        query.ToDynamicPaginateAsync(request.PageRequest.PageIndex, request.PageRequest.PageSize, request.DynamicQuery, defaultOrderBy, tieBreaker,
            TRequest.QueryableFields, defaultDescending, cancellationToken);
}
