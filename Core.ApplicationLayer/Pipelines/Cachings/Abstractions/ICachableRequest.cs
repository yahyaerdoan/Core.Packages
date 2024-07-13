using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ApplicationLayer.Pipelines.Cachings.Abstractions;

public interface ICachableRequest
{
    string CacheKey { get; }
    bool ByPassCache { get; }
    TimeSpan? SlidingExpiration { get; }
}
