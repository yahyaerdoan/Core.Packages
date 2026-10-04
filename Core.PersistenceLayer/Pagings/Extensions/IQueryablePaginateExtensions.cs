using Core.PersistenceLayer.Pagings.Paging;

namespace Core.PersistenceLayer.Pagings.Extensions;

public static class IQueryablePaginateExtensions
{
    public static Paginate<T> ToPaginate<T>(
        this IQueryable<T> source,
        int index,
        int size)
    {
        PagingGuard.Validate(index, size);

        int? skip = PagingGuard.SkipFor(index, size);
        List<T> items = skip is { } rows ? [.. source.Skip(rows).Take(size)] : [];
        int count = PagingGuard.KnownCount(skip, size, items.Count) ?? source.Count();

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
