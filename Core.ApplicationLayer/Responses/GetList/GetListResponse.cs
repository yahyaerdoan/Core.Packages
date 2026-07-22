using Core.PersistenceLayer.Pagings.Paging;

namespace Core.ApplicationLayer.Responses.GetList;

public class GetListResponse<T> : BasePageableModel
{
    private IList<T> items;

    public IList<T> Items
    {
        get => items ??= new List<T>();
        set => items = value;
    }
}
