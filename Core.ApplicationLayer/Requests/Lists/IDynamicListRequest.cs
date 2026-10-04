using Core.ApplicationLayer.Requests.Page;
using Core.PersistenceLayer.Dynamics.Dynamic;

namespace Core.ApplicationLayer.Requests.Lists;

/// <summary>A paged list request with a client filter and sort, validated by <see cref="DynamicListRequestValidator{TRequest}"/>.</summary>
public interface IDynamicListRequest
{
    PageRequest PageRequest { get; }

    DynamicQuery? DynamicQuery { get; }

    /// <summary>The only fields the client may filter and sort on; everything else is rejected with 400.</summary>
    static abstract IReadOnlySet<string> QueryableFields { get; }
}
