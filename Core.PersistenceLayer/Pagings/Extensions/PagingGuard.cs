namespace Core.PersistenceLayer.Pagings.Extensions;

internal static class PagingGuard
{
    public static void Validate(int index, int size)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(size);
    }

    /// <summary>Rows to skip for the page, or null when the page starts past the largest skip a query can express.</summary>
    public static int? SkipFor(int index, int size)
    {
        long skip = (long)index * size;
        return skip > int.MaxValue ? null : (int)skip;
    }

    public static int Pages(int count, int size) => (int)((count + (long)size - 1) / size);

    /// <summary>The total when the fetched page already proves it (a partly filled page is the last one), so no COUNT query is needed.</summary>
    public static int? KnownCount(int? skip, int size, int itemCount) =>
        skip is { } rows && itemCount < size && (itemCount > 0 || rows == 0) ? rows + itemCount : null;
}
