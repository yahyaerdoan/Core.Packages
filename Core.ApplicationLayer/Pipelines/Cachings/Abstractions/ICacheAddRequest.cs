using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ApplicationLayer.Pipelines.Cachings.Abstractions;

public interface ICacheAddRequest
{
    string CacheKey { get; }
    string? CacheGroupKey { get; }
    bool ByPassCache { get; }
    TimeSpan? SlidingExpiration { get; }
}
