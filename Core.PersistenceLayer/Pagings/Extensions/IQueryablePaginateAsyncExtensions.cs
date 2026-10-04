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

        int count = await source.CountAsync(cancellationToken).ConfigureAwait(false);
        List<T> items = PagingGuard.SkipFor(index, size) is { } skip
            ? await source.Skip(skip).Take(size).ToListAsync(cancellationToken).ConfigureAwait(false)
            : [];

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
