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

        int count = source.Count();
        List<T> items = PagingGuard.SkipFor(index, size) is { } skip ? [.. source.Skip(skip).Take(size)] : [];

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
