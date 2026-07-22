using Core.PersistenceLayer.Pagings.Paging;

namespace Core.PersistenceLayer.Pagings.Extensions;

public static class IQueryablePaginateExtensions
{
    public static Paginate<T> ToPaginate<T>(
        this IQueryable<T> source,
        int index,
        int size,
        CancellationToken cancellationToken = default)
    {
        int count = source.Count();
        List<T> items = source.Skip(index * size).Take(size).ToList();
        Paginate<T> result = new Paginate<T>()
        {
            Index = index,
            Count = count,
            Items = items,
            Size = size,
            Pages = (int)Math.Ceiling(count / (double)size)

        };
        return result;
    }
}
