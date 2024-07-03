using Core.PersistenceLayer.Pagings.Paging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.PersistenceLayer.Pagings.Extensions;

public static class IQueryablePaginateAsyncExtensions
{
    public static async Task<Paginate<T>> ToPaginateAsync<T>(
        this IQueryable<T> source,
        int index,
        int size,
        CancellationToken cancellationToken = default)
    {
        int count = await source.CountAsync(cancellationToken).ConfigureAwait(false);
        List<T> items = await source.Skip(index *  size).Take(size).ToListAsync().ConfigureAwait(false);
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
