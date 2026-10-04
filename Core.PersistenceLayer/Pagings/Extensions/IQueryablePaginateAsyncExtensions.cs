using Core.PersistenceLayer.Pagings.Paging;
using Microsoft.EntityFrameworkCore;

namespace Core.PersistenceLayer.Pagings.Extensions;

public static class IQueryablePaginateAsyncExtensions
{
    public static async Task<Paginate<T>> ToPaginateAsync<T>(
        this IQueryable<T> source,
        int index,
        int size,
        CancellationToken cancellationToken = default)
    {
        PagingGuard.Validate(index, size);

        int? skip = PagingGuard.SkipFor(index, size);
        List<T> items = skip is { } rows
            ? await source.Skip(rows).Take(size).ToListAsync(cancellationToken).ConfigureAwait(false)
            : [];
        int count = PagingGuard.KnownCount(skip, size, items.Count) ?? await source.CountAsync(cancellationToken).ConfigureAwait(false);

        return new Paginate<T>
        {
            Index = index,
            Count = count,
            Items = items,
            Size = size,
            Pages = PagingGuard.Pages(count, size),
        };
    }
}
