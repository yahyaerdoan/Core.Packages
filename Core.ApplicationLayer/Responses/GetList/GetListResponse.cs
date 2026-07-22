using Core.PersistenceLayer.Pagings.Paging;

namespace Core.ApplicationLayer.Responses.GetList;

public class GetListResponse<T> : BasePageableModel
{
    public IList<T> Items { get; set; } = [];
}
